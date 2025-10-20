// -----------------------------------------------------------------------
// <copyright file="Simulator.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// NOTICE: THIS FILE HAS BEEN MODIFIED BY AillieoTech UNDER COMPLIANCE WITH THE APACHE 2.0 LICENCE FROM THE ORIGINAL WORK.
// THE FOLLOWING IS THE COPYRIGHT OF THE ORIGINAL DOCUMENT:

/*
 * Simulator.cs
 * RVO2 Library C#
 *
 * SPDX-FileCopyrightText: 2008 University of North Carolina at Chapel Hill
 * SPDX-License-Identifier: Apache-2.0
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Please send all bug reports to <geom@cs.unc.edu>.
 *
 * The authors may be contacted via:
 *
 * Jur van den Berg, Stephen J. Guy, Jamie Snape, Ming C. Lin, Dinesh Manocha
 * Dept. of Computer Science
 * 201 S. Columbia St.
 * Frederick P. Brooks, Jr. Computer Science Bldg.
 * Chapel Hill, N.C. 27599-3175
 * United States of America
 *
 * <http://gamma.cs.unc.edu/RVO2/>
 */

using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RVO
{
	/// <summary>
	///     Defines the simulation.
	/// </summary>
	public sealed class Simulator : IDisposable
	{
		private NativeList<Agent> agents;
		private NativeList<Obstacle> obstacles;
		private KdTree kdTree;
		private float timeStep;

		private NativeParallelHashMap<int, int> agentIndexLookup;
		private NativeParallelMultiHashMap<int, int> obstacleIndexLookup;
		private int sid;

		private Agent defaultAgent;

		private int numWorkers;

		private JobHandle jobHandle;

		private float globalTime;
		private bool disposedValue;

		private bool agentTreeDirty;
		private bool obstacleTreeDirty;

		/// <summary>
		///     Initializes a new instance of the <see cref="Simulator" /> class.
		/// </summary>
		public Simulator()
		{
			agents = new NativeList<Agent>(8, Allocator.Persistent);
			obstacles = new NativeList<Obstacle>(8, Allocator.Persistent);
			kdTree = new KdTree(0, 0);
			agentIndexLookup = new NativeParallelHashMap<int, int>(8, Allocator.Persistent);
			obstacleIndexLookup = new NativeParallelMultiHashMap<int, int>(8, Allocator.Persistent);

			Clear();
		}

		/// <summary>
		///     Adds a new agent with default properties to the simulation.
		/// </summary>
		/// <param name="position">
		///     The two-dimensional starting position
		///     of this agent.
		/// </param>
		/// <returns>
		///     The number of the agent, or -1 when the agent defaults
		///     have not been set.
		/// </returns>
		public int AddAgent(float2 position)
		{
			unsafe
			{
				fixed(Agent* defaultAgentPrt = &defaultAgent)
				{
					return AddAgent(
						position,
						defaultAgentPrt->neighborDist,
						defaultAgentPrt->maxNeighbors,
						defaultAgentPrt->timeHorizon,
						defaultAgentPrt->timeHorizonObst,
						defaultAgentPrt->radius,
						defaultAgentPrt->maxSpeed,
						defaultAgentPrt->velocity);
				}
			}
		}

		/// <summary>
		///     Adds a new obstacle to the simulation.
		/// </summary>
		/// <param name="vertices">
		///     List of the vertices of the polygonal obstacle
		///     in counterclockwise order.
		/// </param>
		/// <returns>
		///     The number of the first vertex of the obstacle, or -1 when
		///     the number of vertices is less than two.
		/// </returns>
		/// <remarks>
		///     To add a "negative" obstacle, e.g. a bounding polygon around
		///     the environment, the vertices should be listed in clockwise order.
		/// </remarks>
		public int AddObstacle(IList<float2> vertices)
		{
			if(vertices.Count < 2)
			{
				return -1;
			}

			var obstacleId = ++sid;
			var startIndex = obstacles.Length;

			unsafe
			{
				for(var i = 0; i < vertices.Count; ++i)
				{
					// NewObstacleVert will cause this.obstacles change
					// so the old obstaclesPtr value became invalid and we have to assign again.
					var obstacle = NewObstacleVert(vertices[i], obstacleId);
					var obstaclesPtr = obstacles.GetUnsafePtr();
					var obstacleIndex = obstacle->id;

					var isFirst = i == 0;
					var isLast = i == vertices.Count - 1;

					if(!isFirst)
					{
						obstacle->previousIndex = obstacleIndex - 1;
						var previous = obstaclesPtr + obstacle->previousIndex;
						previous->nextIndex = obstacleIndex;
					}

					if(isLast)
					{
						obstacle->nextIndex = startIndex;
						var next = obstaclesPtr + startIndex;
						next->previousIndex = startIndex;
					}

					obstacle->direction = math.normalize(vertices[isLast ? 0 : i + 1] - vertices[i]);

					if(vertices.Count == 2)
					{
						obstacle->convex = true;
					}
					else
					{
						obstacle->convex = RVOMath.LeftOf(
							vertices[isFirst ? vertices.Count - 1 : i - 1],
							vertices[i],
							vertices[isLast ? 0 : i + 1]) >= 0f;
					}
				}
			}

			obstacleTreeDirty = true;

			return obstacleId;
		}

		/// <summary>
		///     Remove a agent.
		/// </summary>
		/// <param name="agentId">The agent to remove.</param>
		/// <returns>If the agent was found and removed or not.</returns>
		public bool RemoveAgent(int agentId)
		{
			if(!agentIndexLookup.TryGetValue(agentId, out var index))
			{
				return false;
			}

			var lastIndex = agents.Length - 1;
			var lastId = agents[lastIndex].id;
			agents.RemoveAtSwapBack(index);
			agentIndexLookup.Remove(agentId);
			agentIndexLookup[lastId] = index;

			agentTreeDirty = true;

			return true;
		}

		/// <summary>
		///     Remove agentIds.
		/// </summary>
		/// <param name="agentIds">The agentIds to remove.</param>
		/// <returns>The number of key-value pairs that were found and removed.</returns>
		public int RemoveAgents(IEnumerable<int> agentIds)
		{
			var removed = 0;
			foreach(var agentId in agentIds)
			{
				if(!agentIndexLookup.TryGetValue(agentId, out var index))
				{
					continue;
				}

				removed++;

				var lastIndex = agents.Length - 1;
				var lastId = agents[lastIndex].id;
				agents.RemoveAtSwapBack(index);
				agentIndexLookup.Remove(agentId);
				agentIndexLookup[lastId] = index;
			}

			agentTreeDirty = true;

			return removed;
		}

		/// <summary>
		///     Remove an obstacle.
		/// </summary>
		/// <param name="obstacleId">The obstacle to remove.</param>
		/// <returns>If the obstacle was found and removed or not.</returns>
		public bool RemoveObstacle(int obstacleId)
		{
			var buffer = new NativeList<int>(Allocator.Temp);
			obstacleIndexLookup.GetValuesForKey(obstacleId, ref buffer);
			buffer.Sort();

			if(buffer.IsCreated && buffer.Length > 0)
			{
				for(var i = buffer.Length - 1; i >= 0; --i)
				{
					var index = buffer[i];
					var lastIndex = obstacles.Length - 1;
					var lastObstacle = obstacles[lastIndex];
					var lastGroup = lastObstacle.obstacle;

					var removingLast = index == lastIndex;
					if(removingLast)
					{
						var lastObstacleNext = obstacles[lastObstacle.nextIndex];
						lastObstacleNext.previousIndex = lastObstacle.previousIndex;
						obstacles[lastObstacle.nextIndex] = lastObstacleNext;

						var lastObstaclePrev = obstacles[lastObstacle.previousIndex];
						lastObstaclePrev.nextIndex = lastObstacle.nextIndex;
						obstacles[lastObstacle.previousIndex] = lastObstaclePrev;
					}
					else
					{
						var lastObstacleNext = obstacles[lastObstacle.nextIndex];
						lastObstacleNext.previousIndex = index;
						obstacles[lastObstacle.nextIndex] = lastObstacleNext;

						var lastObstaclePrev = obstacles[lastObstacle.previousIndex];
						lastObstaclePrev.nextIndex = index;
						obstacles[lastObstacle.previousIndex] = lastObstaclePrev;
					}

					obstacleIndexLookup.Replace(lastGroup, lastIndex, index);
					obstacles.RemoveAtSwapBack(index);
				}
			}

			buffer.Dispose();

			obstacleIndexLookup.Remove(obstacleId);

			obstacleTreeDirty = true;

			return true;
		}

		/// <summary>
		///     Ensure the simulator completes its current job
		///     so that the agent and obstacle data can be safely accessed.
		/// </summary>
		public void EnsureCompleted()
		{
			jobHandle.Complete();
			jobHandle = default;
		}

		/// <summary>
		///     Clears the simulation.
		/// </summary>
		public void Clear()
		{
			EnsureCompleted();

			if(agents.IsCreated && agents.Length > 0)
			{
				agents.Clear();
			}

			defaultAgent = default;

			kdTree.Clear();

			if(obstacles.IsCreated && obstacles.Length > 0)
			{
				obstacles.Clear();
			}

			globalTime = 0f;
			timeStep = 0.1f;

			SetNumWorkers(0);

			agentTreeDirty = false;
			obstacleTreeDirty = false;
		}

		/// <summary>
		///     Performs a simulation step and updates the two-dimensional
		///     position and two-dimensional velocity of each agent.
		/// </summary>
	public void DoStep()
	{
		EnsureCompleted();

		EnsureObstacleTree();

		var arrayLength = agents.Length;

		EnsureTreeCapacity(ref kdTree, arrayLength);

		var effectiveTimeStep = timeStep * Time.timeScale;

		// job0
		var buildJob = new BuildJob(kdTree, agents.AsParallelReader());
		var jobHandle0 = buildJob.Schedule();

		// job1
		var innerLoop = Mathf.Max(arrayLength / Mathf.Max(numWorkers, 1), 1);
		var agentResult = new NativeArray<float2>(agents.Length, Allocator.TempJob);
		var computeJob = new ComputeJob(
			agents.AsParallelReader(),
			obstacles.AsParallelReader(),
			kdTree.AsParallelReader(),
			effectiveTimeStep,
			agentResult);
		var jobHandle1 = computeJob.Schedule(arrayLength, innerLoop, jobHandle0);

		// job2
		var updateJob = new UpdateJob(agents, effectiveTimeStep, agentResult);
		var jobHandle2 = updateJob.Schedule(arrayLength, innerLoop, jobHandle1);
		agentResult.Dispose(jobHandle2);

		jobHandle = jobHandle2;
		globalTime += effectiveTimeStep;
	}

		/// <summary>
		///     Returns the specified agent neighbor of the specified agent.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose agent neighbor
		///     is to be retrieved.
		/// </param>
		/// <param name="neighborId">The number of the agent neighbor to be retrieved.</param>
		/// <returns>The number of the neighboring agent.</returns>
		public int GetAgentAgentNeighbor(int agentId, int neighborId)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Returns the maximum neighbor count of a specified agent.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose maximum neighbor
		///     count is to be retrieved.
		/// </param>
		/// <returns>The present maximum neighbor count of the agent.</returns>
		public int GetAgentMaxNeighbors(int agentId)
		{
			var index = agentIndexLookup[agentId];
			return agents[index].maxNeighbors;
		}

		/// <summary>
		///     Returns the maximum speed of a specified agent.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose maximum speed
		///     is to be retrieved.
		/// </param>
		/// <returns>The present maximum speed of the agent.</returns>
		public float GetAgentMaxSpeed(int agentId)
		{
			var index = agentIndexLookup[agentId];
			return agents[index].maxSpeed;
		}

		/// <summary>
		///     Returns the maximum neighbor distance of a specified agent.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose maximum neighbor
		///     distance is to be retrieved.
		/// </param>
		/// <returns>The present maximum neighbor distance of the agent.</returns>
		public float GetAgentNeighborDist(int agentId)
		{
			var index = agentIndexLookup[agentId];
			return agents[index].neighborDist;
		}

		/// <summary>
		///     Returns the count of agent neighbors taken into account to
		///     compute the current velocity for the specified agent.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose count of agent
		///     neighbors is to be retrieved.
		/// </param>
		/// <returns>
		///     The count of agent neighbors taken into account to compute
		///     the current velocity for the specified agent.
		/// </returns>
		public int GetAgentNumAgentNeighbors(int agentId)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Returns the count of obstacle neighbors taken into account
		///     to compute the current velocity for the specified agent.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose count of obstacle
		///     neighbors is to be retrieved.
		/// </param>
		/// <returns>
		///     The count of obstacle neighbors taken into account to
		///     compute the current velocity for the specified agent.
		/// </returns>
		public int GetAgentNumObstacleNeighbors(int agentId)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Returns the specified obstacle neighbor of the specified agent.
		/// </summary>
		/// <returns>
		///     The number of the first vertex of the neighboring
		///     obstacle edge.
		/// </returns>
		/// <param name="agentId">
		///     The number of the agent whose obstacle neighbor
		///     is to be retrieved.
		/// </param>
		/// <param name="neighborId">
		///     The number of the obstacle neighbor to be
		///     retrieved.
		/// </param>
		public int GetAgentObstacleNeighbor(int agentId, int neighborId)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Returns the two-dimensional position of a specified agent.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose two-dimensional
		///     position is to be retrieved.
		/// </param>
		/// <returns>The present two-dimensional position of the(center of the) agent.</returns>
		public float2 GetAgentPosition(int agentId)
		{
			var index = agentIndexLookup[agentId];
			return agents[index].position;
		}

		/// <summary>
		///     Returns the two-dimensional preferred velocity of a specified agent.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose two-dimensional
		///     preferred velocity is to be retrieved.
		/// </param>
		/// <returns>The present two-dimensional preferred velocity of the agent.</returns>
		public float2 GetAgentPrefVelocity(int agentId)
		{
			var index = agentIndexLookup[agentId];
			return agents[index].prefVelocity;
		}

		/// <summary>
		///     Returns the radius of a specified agent.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose radius is to be
		///     retrieved.
		/// </param>
		/// <returns>The present radius of the agent.</returns>
		public float GetAgentRadius(int agentId)
		{
			var index = agentIndexLookup[agentId];
			return agents[index].radius;
		}

		/// <summary>
		///     Returns the time horizon of a specified agent.
		/// </summary>
		/// <returns>The present time horizon of the agent.</returns>
		/// <param name="agentId">
		///     The number of the agent whose time horizon is
		///     to be retrieved.
		/// </param>
		public float GetAgentTimeHorizon(int agentId)
		{
			var index = agentIndexLookup[agentId];
			return agents[index].timeHorizon;
		}

		/// <summary>
		///     Returns the time horizon with respect to obstacles of a specified agent.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose time horizon with
		///     respect to obstacles is to be retrieved.
		/// </param>
		/// <returns>The present time horizon with respect to obstacles of the agent.</returns>
		public float GetAgentTimeHorizonObst(int agentId)
		{
			var index = agentIndexLookup[agentId];
			return agents[index].timeHorizonObst;
		}

		/// <summary>
		///     Returns the two-dimensional linear velocity of a specified agent.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose two-dimensional
		///     linear velocity is to be retrieved.
		/// </param>
		/// <returns>The present two-dimensional linear velocity of the agent.</returns>
		public float2 GetAgentVelocity(int agentId)
		{
			var index = agentIndexLookup[agentId];
			return agents[index].velocity;
		}

		/// <summary>
		///     Returns the global time of the simulation.
		/// </summary>
		/// <returns>
		///     The present global time of the simulation (zero initially).
		/// </returns>
		public float GetGlobalTime()
		{
			return globalTime;
		}

		/// <summary>
		///     Returns the count of agentIds in the simulation.
		/// </summary>
		/// <returns>The count of agentIds in the simulation.</returns>
		public int GetNumAgents()
		{
			return agents.Length;
		}

		/// <summary>
		///     Returns the count of obstacle vertices in the simulation.
		/// </summary>
		/// <returns>The count of obstacle vertices in the simulation.</returns>
		public int GetNumObstacleVertices()
		{
			return obstacles.Length;
		}

		/// <summary>
		///     Returns the count of workers.
		/// </summary>
		/// <returns>The count of workers.</returns>
		public int GetNumWorkers()
		{
			return numWorkers;
		}

		/// <summary>
		///     Returns the two-dimensional position of a specified obstacle vertex.
		/// </summary>
		/// <param name="vertexId">The number of the obstacle vertex to be retrieved.</param>
		/// <returns>The two-dimensional position of the specified obstacle vertex.</returns>
		public float2 GetObstacleVertex(int vertexId)
		{
			return obstacles[vertexId].point;
		}

		/// <summary>
		///     Returns the number of the obstacle vertex succeeding the specified
		///     obstacle vertex in its polygon.
		/// </summary>
		/// <param name="vertexId">
		///     The number of the obstacle vertex whose successor
		///     is to be retrieved.
		/// </param>
		/// <returns>
		///     The number of the obstacle vertex succeeding the specified
		///     obstacle vertex in its polygon.
		/// </returns>
		public int GetNextObstacleVertexId(int vertexId)
		{
			return obstacles[vertexId].nextIndex;
		}

		/// <summary>
		///     Returns the number of the obstacle vertex preceding the specified
		///     obstacle vertex in its polygon.
		/// </summary>
		/// <param name="vertexId">
		///     The number of the obstacle vertex whose
		///     predecessor is to be retrieved.
		/// </param>
		/// <returns>
		///     The number of the obstacle vertex preceding the specified
		///     obstacle vertex in its polygon.
		/// </returns>
		public int GetPrevObstacleVertexId(int vertexId)
		{
			return obstacles[vertexId].previousIndex;
		}

		/// <summary>
		///     Returns the first vertex of one obstacle.
		/// </summary>
		/// <param name="obstacleId">The obstacle.</param>
		/// <returns>The first vertex of the obstacle.</returns>
		public int GetFirstObstacleVertexId(int obstacleId)
		{
			if(obstacleIndexLookup.TryGetFirstValue(obstacleId, out var result, out _))
			{
				return result;
			}

			return -1;
		}

		/// <summary>
		///     Returns the time step of the simulation.
		/// </summary>
		/// <returns>The present time step of the simulation.</returns>
		public float GetTimeStep()
		{
			return timeStep;
		}

		/// <summary>
		///     Performs a visibility query between the two specified points
		///     with respect to the obstacles.
		/// </summary>
		/// <param name="point1">The first point of the query.</param>
		/// <param name="point2">The second point of the query.</param>
		/// <param name="radius">
		///     The minimal distance between the line connecting
		///     the two points and the obstacles in order for the points to be
		///     mutually visible (optional). Must be non-negative.
		/// </param>
		/// <returns>
		///     A boolean specifying whether the two points are mutually
		///     visible. Returns true when the obstacles have not been processed.
		/// </returns>
		public bool QueryVisibility(float2 point1, float2 point2, float radius)
		{
			EnsureCompleted();

			EnsureAgentTree();
			EnsureObstacleTree();

			unsafe
			{
				return kdTree.AsParallelReader()
					.QueryVisibility(
						point1,
						point2,
						radius,
						obstacles.GetUnsafeReadOnlyPtr(),
						obstacles.Length);
			}
		}

		/// <summary>
		///     Sets the default properties for any new agent that is added.
		/// </summary>
		/// <param name="neighborDist">
		///     The default maximum distance (center point
		///     to center point) to other agentIds a new agent takes into account in
		///     the navigation. The larger this number, the longer he running time of
		///     the simulation. If the number is too low, the simulation will not be
		///     safe. Must be non-negative.
		/// </param>
		/// <param name="maxNeighbors">
		///     The default maximum number of other agentIds
		///     a new agent takes into account in the navigation. The larger this
		///     number, the longer the running time of the simulation. If the number
		///     is too low, the simulation will not be safe.
		/// </param>
		/// <param name="timeHorizon">
		///     The default minimal amount of time for
		///     which a new agent's velocities that are computed by the simulation
		///     are safe with respect to other agentIds. The larger this number, the
		///     sooner an agent will respond to the presence of other agentIds, but the
		///     less freedom the agent has in choosing its velocities. Must be
		///     positive.
		/// </param>
		/// <param name="timeHorizonObst">
		///     The default minimal amount of time for
		///     which a new agent's velocities that are computed by the simulation
		///     are safe with respect to obstacles. The larger this number, the
		///     sooner an agent will respond to the presence of obstacles, but the
		///     less freedom the agent has in choosing its velocities. Must be
		///     positive.
		/// </param>
		/// <param name="radius">
		///     The default radius of a new agent. Must be
		///     non-negative.
		/// </param>
		/// <param name="maxSpeed">
		///     The default maximum speed of a new agent. Must
		///     be non-negative.
		/// </param>
		/// <param name="velocity">
		///     The default initial two-dimensional linear
		///     velocity of a new agent.
		/// </param>
		public void SetAgentDefaults(
			float neighborDist,
			int maxNeighbors,
			float timeHorizon,
			float timeHorizonObst,
			float radius,
			float maxSpeed,
			float2 velocity)
		{
			defaultAgent = new Agent
			{
				maxNeighbors = maxNeighbors,
				maxSpeed = maxSpeed,
				neighborDist = neighborDist,
				radius = radius,
				timeHorizon = timeHorizon,
				timeHorizonObst = timeHorizonObst,
				velocity = velocity
			};
		}

		/// <summary>
		///     Sets the maximum neighbor count of a specified agent.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose maximum neighbor
		///     count is to be modified.
		/// </param>
		/// <param name="maxNeighbors">
		///     The replacement maximum neighbor count.
		/// </param>
		public void SetAgentMaxNeighbors(int agentId, int maxNeighbors)
		{
			var index = agentIndexLookup[agentId];
			var agent = agents[index];
			agent.maxNeighbors = maxNeighbors;
			agents[index] = agent;
		}

		/// <summary>
		///     Sets the maximum speed of a specified agent.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose maximum speed is
		///     to be modified.
		/// </param>
		/// <param name="maxSpeed">
		///     The replacement maximum speed. Must be
		///     non-negative.
		/// </param>
		public void SetAgentMaxSpeed(int agentId, float maxSpeed)
		{
			var index = agentIndexLookup[agentId];
			var agent = agents[index];
			agent.maxSpeed = maxSpeed;
			agents[index] = agent;
		}

		/// <summary>
		///     Sets the maximum neighbor distance of a specified agent.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose maximum neighbor
		///     distance is to be modified.
		/// </param>
		/// <param name="neighborDist">
		///     The replacement maximum neighbor distance.
		///     Must be non-negative.
		/// </param>
		public void SetAgentNeighborDist(int agentId, float neighborDist)
		{
			var index = agentIndexLookup[agentId];
			var agent = agents[index];
			agent.neighborDist = neighborDist;
			agents[index] = agent;
		}

		/// <summary>
		///     Sets the two-dimensional position of a specified agent.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose two-dimensional
		///     position is to be modified.
		/// </param>
		/// <param name="position">
		///     The replacement of the two-dimensional
		///     position.
		/// </param>
		public void SetAgentPosition(int agentId, float2 position)
		{
			var index = agentIndexLookup[agentId];
			var agent = agents[index];
			agent.position = position;
			agents[index] = agent;
		}

		/// <summary>
		///     Sets the two-dimensional preferred velocity of a specified agent.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose two-dimensional
		///     preferred velocity is to be modified.
		/// </param>
		/// <param name="prefVelocity">
		///     The replacement of the two-dimensional
		///     preferred velocity.
		/// </param>
		public void SetAgentPrefVelocity(int agentId, float2 prefVelocity)
		{
			var index = agentIndexLookup[agentId];
			var agent = agents[index];
			agent.prefVelocity = prefVelocity;
			agents[index] = agent;
		}

		/// <summary>
		///     Sets the radius of a specified agent.
		/// </summary>
		/// <param name="agentId">The number of the agent whose radius is to be modified.</param>
		/// <param name="radius">The replacement radius. Must be non-negative.</param>
		public void SetAgentRadius(int agentId, float radius)
		{
			var index = agentIndexLookup[agentId];
			var agent = agents[index];
			agent.radius = radius;
			agents[index] = agent;
		}

		/// <summary>
		///     Sets the time horizon of a specified agent with respect to other agentIds.
		/// </summary>
		/// <param name="agentId">The number of the agent whose time horizon is to be modified.</param>
		/// <param name="timeHorizon">
		///     The replacement time horizon with respect to
		///     other agentIds. Must be positive.
		/// </param>
		public void SetAgentTimeHorizon(int agentId, float timeHorizon)
		{
			var index = agentIndexLookup[agentId];
			var agent = agents[index];
			agent.timeHorizon = timeHorizon;
			agents[index] = agent;
		}

		/// <summary>
		///     Sets the time horizon of a specified agent with respect to obstacles.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose time horizon with
		///     respect to obstacles is to be modified.
		/// </param>
		/// <param name="timeHorizonObst">
		///     The replacement time horizon with
		///     respect to obstacles. Must be positive.
		/// </param>
		public void SetAgentTimeHorizonObst(int agentId, float timeHorizonObst)
		{
			var index = agentIndexLookup[agentId];
			var agent = agents[index];
			agent.timeHorizonObst = timeHorizonObst;
			agents[index] = agent;
		}

		/// <summary>
		///     Sets the two-dimensional linear velocity of a specified agent.
		/// </summary>
		/// <param name="agentId">
		///     The number of the agent whose two-dimensional
		///     linear velocity is to be modified.
		/// </param>
		/// <param name="velocity">The replacement two-dimensional linear velocity.</param>
		public void SetAgentVelocity(int agentId, float2 velocity)
		{
			var index = agentIndexLookup[agentId];
			var agent = agents[index];
			agent.velocity = velocity;
			agents[index] = agent;
		}

		/// <summary>
		///     Sets the global time of the simulation.
		/// </summary>
		/// <param name="globalTime">The global time of the simulation.</param>
		public void SetGlobalTime(float globalTime)
		{
			this.globalTime = globalTime;
		}

		/// <summary>
		///     Sets the number of workers.
		/// </summary>
		/// <param name="numWorkers">The number of workers.</param>
		public void SetNumWorkers(int numWorkers)
		{
			this.numWorkers = numWorkers;

			if(this.numWorkers <= 0)
			{
				ThreadPool.GetMinThreads(out this.numWorkers, out _);
			}
		}

		/// <summary>
		///     Sets the time step of the simulation.
		/// </summary>
		/// <param name="timeStep">
		///     The time step of the simulation.
		///     Must be positive.
		/// </param>
		public void SetTimeStep(float timeStep)
		{
			this.timeStep = timeStep;
		}

		/// <summary>
		///     Queries the agent tree for agentIds within a specified radius of a given point.
		/// </summary>
		/// <param name="point">The center point of the query.</param>
		/// <param name="radius">The radius of the query.</param>
		/// <param name="result">The list to store the agentIds found.</param>
		/// <returns>The number of agentIds found within the specified radius.</returns>
		public int QueryAgent(float2 point, float radius, List<int> result)
		{
			if(result == null)
			{
				throw new ArgumentNullException(nameof(result));
			}

			EnsureCompleted();

			EnsureAgentTree();

			NativeArray<Agent> agentsAsArray = agents;

			var buffer = new UnsafeList<Agent>(8, Allocator.Temp);
			kdTree.AsParallelReader()
				.QueryAgentTree(
					in point,
					in radius,
					in agentsAsArray,
					ref buffer);

			result.Clear();
			foreach(var a in buffer)
			{
				result.Add(a.id);
			}

			buffer.Dispose();

			return result.Count;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///     Finalizes an instance of the <see cref="Simulator" /> class.
		/// </summary>
		~Simulator()
		{
			Dispose(false);
		}

		/// <summary>
		///     Recursive method for building an agent k-D tree.
		/// </summary>
		/// <param name="kdTree">The k-D tree.</param>
		/// <param name="begin">The beginning agent k-D tree node node index.</param>
		/// <param name="end">The ending agent k-D tree node index.</param>
		/// <param name="nodeIndex">The current agent k-D tree node index.</param>
		/// <param name="agents">The array that holds the agent data.</param>
		/// <param name="agentsLength">The length for array <paramref name="agents" />.</param>
		private static unsafe void BuildAgentTreeRecursive(
			ref KdTree kdTree,
			int begin,
			int end,
			int nodeIndex,
			Agent* agents,
			int agentsLength)
		{
			var agentTreePtr = (KdTree.AgentTreeNode*)kdTree.agentTree.GetUnsafePtr();
			var agentIdsPtr = (int*)kdTree.agentIds.GetUnsafePtr();

			var node = agentTreePtr + nodeIndex;
			node->begin = begin;
			node->end = end;
			var agentBegin = agents + agentIdsPtr[begin];
			node->minX = node->maxX = agentBegin->position.x;
			node->minY = node->maxY = agentBegin->position.y;

			for(var i = begin + 1; i < end; ++i)
			{
				var agentI = agents + agentIdsPtr[i];
				node->maxX = math.max(node->maxX, agentI->position.x);
				node->minX = math.min(node->minX, agentI->position.x);
				node->maxY = math.max(node->maxY, agentI->position.y);
				node->minY = math.min(node->minY, agentI->position.y);
			}

			if(end - begin <= KdTree.MaxLeafSize)
			{
				return;
			}

			// No leaf node.
			var isVertical = node->maxX - node->minX
			                 > node->maxY - node->minY;
			var splitValue = 0.5f * (isVertical
				? node->maxX + node->minX
				: node->maxY + node->minY);

			var left = begin;
			var right = end;

			while(left < right)
			{
				while(true)
				{
					var agentLeft = agents + agentIdsPtr[left];
					if(left < right
					   && (isVertical ? agentLeft->position.x : agentLeft->position.y) < splitValue)
					{
						++left;
					}
					else
					{
						break;
					}
				}

				while(true)
				{
					var agentRight = agents + agentIdsPtr[right - 1];
					if(right > left
					   && (isVertical ? agentRight->position.x : agentRight->position.y) >= splitValue)
					{
						--right;
					}
					else
					{
						break;
					}
				}

				if(left >= right)
				{
					continue;
				}

				var tempAgentIndex = agentIdsPtr[left];
				agentIdsPtr[left] = agentIdsPtr[right - 1];
				agentIdsPtr[right - 1] = tempAgentIndex;
				++left;
				--right;
			}

			var leftSize = left - begin;

			if(leftSize == 0)
			{
				++leftSize;
				++left;
			}

			node->left = nodeIndex + 1;
			node->right = nodeIndex + 2 * leftSize;

			BuildAgentTreeRecursive(ref kdTree, begin, left, node->left, agents, agentsLength);
			BuildAgentTreeRecursive(ref kdTree, left, end, node->right, agents, agentsLength);
		}

		private static void EnsureTreeCapacity(ref KdTree kdTree, int agentCount)
		{
			if(kdTree.agentIds.Length == agentCount)
			{
				return;
			}

			kdTree.agentIds.Resize(agentCount);
			for(var i = 0; i < agentCount; ++i)
			{
				kdTree.agentIds[i] = i;
			}

			var agentTreeSize = 2 * agentCount;
			kdTree.agentTree.Resize(agentTreeSize);
			for(var i = 0; i < agentTreeSize; ++i)
			{
				kdTree.agentTree[i] = default;
			}
		}

		/// <summary>
		///     Builds an agent k-D tree.
		/// </summary>
		/// <param name="kdTree">The k-D Tree.</param>
		/// <param name="agents">The array that holds the agent data.</param>
		/// <param name="agentsLength">The length for array <paramref name="agents" />.</param>
		private static unsafe void BuildAgentTree(
			ref KdTree kdTree,
			Agent* agents,
			int agentsLength)
		{
			if(kdTree.agentIds.Length == 0)
			{
				return;
			}

			for(var i = 0; i < agentsLength; i++)
			{
				kdTree.agentIds[i] = i;
			}

			BuildAgentTreeRecursive(ref kdTree, 0, kdTree.agentIds.Length, 0, agents, agentsLength);
		}

		private unsafe Obstacle* NewObstacleVert(float2 point, int obstacleId)
		{
			var newIndex = obstacles.Length;
			var obstacleVert = new Obstacle(newIndex, point, obstacleId);
			obstacles.Add(obstacleVert);

			obstacleIndexLookup.Add(obstacleId, newIndex);

			return obstacles.GetUnsafePtr() + newIndex;
		}

		private unsafe void EnsureAgentTree()
		{
			if(agentTreeDirty)
			{
				EnsureTreeCapacity(ref kdTree, agents.Length);
				var agentsReadonly = agents.GetUnsafeReadOnlyPtr();
				var agentsLength = agents.Length;
				BuildAgentTree(ref kdTree, agentsReadonly, agentsLength);
				agentTreeDirty = false;
			}
		}

		private void EnsureObstacleTree()
		{
			if(obstacleTreeDirty)
			{
				BuildObstacleTree();
				obstacleTreeDirty = false;
			}
		}

		/// <summary>
		///     Adds a new agent to the simulation.
		/// </summary>
		/// <param name="position">
		///     The two-dimensional starting position of this
		///     agent.
		/// </param>
		/// <param name="neighborDist">
		///     The maximum distance (center point to
		///     center point) to other agentIds this agent takes into account in the
		///     navigation. The larger this number, the longer the running time of
		///     the simulation. If the number is too low, the simulation will not be
		///     safe. Must be non-negative.
		/// </param>
		/// <param name="maxNeighbors">
		///     The maximum number of other agentIds this
		///     agent takes into account in the navigation. The larger this number,
		///     the longer the running time of the simulation. If the number is too
		///     low, the simulation will not be safe.
		/// </param>
		/// <param name="timeHorizon">
		///     The minimal amount of time for which this
		///     agent's velocities that are computed by the simulation are safe with
		///     respect to other agentIds. The larger this number, the sooner this
		///     agent will respond to the presence of other agentIds, but the less
		///     freedom this agent has in choosing its velocities. Must be positive.
		/// </param>
		/// <param name="timeHorizonObst">
		///     The minimal amount of time for which
		///     this agent's velocities that are computed by the simulation are safe
		///     with respect to obstacles. The larger this number, the sooner this
		///     agent will respond to the presence of obstacles, but the less freedom
		///     this agent has in choosing its velocities. Must be positive.
		/// </param>
		/// <param name="radius">
		///     The radius of this agent. Must be non-negative.
		/// </param>
		/// <param name="maxSpeed">
		///     The maximum speed of this agent. Must be
		///     non-negative.
		/// </param>
		/// <param name="velocity">
		///     The initial two-dimensional linear velocity of
		///     this agent.
		/// </param>
		/// <returns>The number of the agent.</returns>
		private unsafe int AddAgent(
			float2 position,
			float neighborDist,
			int maxNeighbors,
			float timeHorizon,
			float timeHorizonObst,
			float radius,
			float maxSpeed,
			float2 velocity)
		{
			var agent = NewAgent();
			agent->maxNeighbors = maxNeighbors;
			agent->maxSpeed = maxSpeed;
			agent->neighborDist = neighborDist;
			agent->position = position;
			agent->radius = radius;
			agent->timeHorizon = timeHorizon;
			agent->timeHorizonObst = timeHorizonObst;
			agent->velocity = velocity;

			return agent->id;
		}

		/// <summary>
		///     Builds an obstacle k-D tree.
		/// </summary>
		private unsafe void BuildObstacleTree()
		{
			var kdTreeToBuild = kdTree;
			kdTreeToBuild.obstacleTreeNodes.Resize(0);

			var obstaclesLength = obstacles.Length;
			var obstacleIds = new NativeArray<int>(obstaclesLength, Allocator.Temp);
			var pointer = (int*)obstacleIds.GetUnsafePtr();

			for(var i = 0; i < obstaclesLength; ++i)
			{
				pointer[i] = i;
			}

			BuildObstacleTreeRecursive(ref kdTreeToBuild, pointer, obstaclesLength);
			kdTree = kdTreeToBuild;

			obstacleIds.Dispose();
		}

		/// <summary>
		///     Recursive method for building an obstacle k-D tree.
		/// </summary>
		/// <param name="kdTreeToBuild">The k-D tree.</param>
		/// <param name="obstacleIds">A pointer to the array holds obstacle Ids.</param>
		/// <param name="obstacleLength">Length of the array holds obstacle Ids.</param>
		/// <returns>An obstacle k-D tree node.</returns>
		private unsafe int BuildObstacleTreeRecursive(
			ref KdTree kdTreeToBuild,
			int* obstacleIds,
			in int obstacleLength)
		{
			if(obstacleLength == 0)
			{
				return -1;
			}

			var nodeIndex = kdTreeToBuild.NewObstacleTreeNode();
			var obstacleTreeNodesPtr = (KdTree.ObstacleTreeNode*)kdTreeToBuild.obstacleTreeNodes.GetUnsafePtr();
			var node = obstacleTreeNodesPtr + nodeIndex;
			kdTree = kdTreeToBuild;

			var optimalSplit = 0;
			var minLeft = obstacleLength;
			var minRight = obstacleLength;

			var obstaclesPtr = obstacles.GetUnsafePtr();

			for(var i = 0; i < obstacleLength; ++i)
			{
				var leftSize = 0;
				var rightSize = 0;

				var obstacleI1Index = obstacleIds[i];
				var obstacleI1 = obstaclesPtr + obstacleI1Index;
				var obstacleI2Index = obstacleI1->nextIndex;
				var obstacleI2 = obstaclesPtr + obstacleI2Index;

				// Compute optimal split node.
				for(var j = 0; j < obstacleLength; ++j)
				{
					if(i == j)
					{
						continue;
					}

					var obstacleJ1Index = obstacleIds[j];
					var obstacleJ1 = obstaclesPtr + obstacleJ1Index;
					var obstacleJ2Index = obstacleJ1->nextIndex;
					var obstacleJ2 = obstaclesPtr + obstacleJ2Index;

					var j1LeftOfI = RVOMath.LeftOf(obstacleI1->point, obstacleI2->point, obstacleJ1->point);
					var j2LeftOfI = RVOMath.LeftOf(obstacleI1->point, obstacleI2->point, obstacleJ2->point);

					if(j1LeftOfI >= -RVOMath.RVO_EPSILON && j2LeftOfI >= -RVOMath.RVO_EPSILON)
					{
						++leftSize;
					}
					else if(j1LeftOfI <= RVOMath.RVO_EPSILON && j2LeftOfI <= RVOMath.RVO_EPSILON)
					{
						++rightSize;
					}
					else
					{
						++leftSize;
						++rightSize;
					}

					var bound1 = new float2(math.max(leftSize, rightSize), math.min(leftSize, rightSize));
					var bound2 = new float2(math.max(minLeft, minRight), math.min(minLeft, minRight));

					if(RVOMath.GreaterEqual(bound1, bound2))
					{
						break;
					}
				}

				var bound1f = new float2(math.max(leftSize, rightSize), math.min(leftSize, rightSize));
				var bound2f = new float2(math.max(minLeft, minRight), math.min(minLeft, minRight));

				if(!RVOMath.Less(bound1f, bound2f))
				{
					continue;
				}

				minLeft = leftSize;
				minRight = rightSize;
				optimalSplit = i;
			}

			{
				// Build split node.
				var leftObstacles = new NativeArray<int>(minLeft, Allocator.Temp);
				var leftObstaclesPtr = (int*)leftObstacles.GetUnsafePtr();

				for(var n = 0; n < minLeft; ++n)
				{
					leftObstaclesPtr[n] = -1;
				}

				var rightObstacles = new NativeArray<int>(minRight, Allocator.Temp);
				var rightObstaclesPtr = (int*)rightObstacles.GetUnsafePtr();

				for(var n = 0; n < minRight; ++n)
				{
					rightObstaclesPtr[n] = -1;
				}

				var leftCounter = 0;
				var rightCounter = 0;
				var i = optimalSplit;

				var obstacleI1Index = obstacleIds[i];
				var obstacleI1 = obstaclesPtr + obstacleI1Index;
				var obstacleI2Index = obstacleI1->nextIndex;
				var obstacleI2 = obstaclesPtr + obstacleI2Index;

				for(var j = 0; j < obstacleLength; ++j)
				{
					if(i == j)
					{
						continue;
					}

					var obstacleJ1Index = obstacleIds[j];
					var obstacleJ1 = obstaclesPtr + obstacleJ1Index;
					var obstacleJ2Index = obstacleJ1->nextIndex;
					var obstacleJ2 = obstaclesPtr + obstacleJ2Index;

					var j1LeftOfI = RVOMath.LeftOf(obstacleI1->point, obstacleI2->point, obstacleJ1->point);
					var j2LeftOfI = RVOMath.LeftOf(obstacleI1->point, obstacleI2->point, obstacleJ2->point);

					if(j1LeftOfI >= -RVOMath.RVO_EPSILON && j2LeftOfI >= -RVOMath.RVO_EPSILON)
					{
						leftObstaclesPtr[leftCounter++] = obstacleIds[j];
					}
					else if(j1LeftOfI <= RVOMath.RVO_EPSILON && j2LeftOfI <= RVOMath.RVO_EPSILON)
					{
						rightObstaclesPtr[rightCounter++] = obstacleIds[j];
					}
					else
					{
						// Split obstacle j.
						var dI2I1 = obstacleI2->point - obstacleI1->point;
						var t = RVOMath.Det(dI2I1, obstacleJ1->point - obstacleI1->point)
						        / RVOMath.Det(dI2I1, obstacleJ1->point - obstacleJ2->point);

						var splitPoint = obstacleJ1->point + t * (obstacleJ2->point - obstacleJ1->point);

						// NewObstacleVert will cause this.obstacles change
						// so the old obstaclesPtr value became invalid and we have to assign again.
						var newObstacle = NewObstacleVert(splitPoint, obstacleJ1->obstacle);
						obstaclesPtr = obstacles.GetUnsafePtr();

						// 往NativeList中添加新元素可能导致NativeList扩容
						// NativeList扩容后,指针指向了新地址
						// 这时通过原指针计算出的其他指针还指向旧地址
						// 需要更新这些指针
						obstacleI1 = obstaclesPtr + obstacleI1Index;
						obstacleI2 = obstaclesPtr + obstacleI2Index;
						obstacleJ1 = obstaclesPtr + obstacleJ1Index;
						obstacleJ2 = obstaclesPtr + obstacleJ2Index;

						var newObstacleIndex = newObstacle->id;
						newObstacle->previousIndex = obstacleJ1Index;
						newObstacle->nextIndex = obstacleJ2Index;
						newObstacle->convex = true;
						newObstacle->direction = obstacleJ1->direction;

						obstacleJ1->nextIndex = newObstacleIndex;
						obstacleJ2->previousIndex = newObstacleIndex;

						if(j1LeftOfI > 0f)
						{
							leftObstaclesPtr[leftCounter++] = obstacleJ1Index;
							rightObstaclesPtr[rightCounter++] = newObstacleIndex;
						}
						else
						{
							rightObstaclesPtr[rightCounter++] = obstacleJ1Index;
							leftObstaclesPtr[leftCounter++] = newObstacleIndex;
						}
					}
				}

				node->obstacleIndex = obstacleI1Index;

				var leftIndex = BuildObstacleTreeRecursive(
					ref kdTreeToBuild,
					(int*)leftObstacles.GetUnsafePtr(),
					leftObstacles.Length);

				// NewObstacleTreeNode() in BuildObstacleTreeRecursive() will cause obstacleTreeNodes change
				// so the old obstacleTreeNodesPtr value became invalid and we have to assign again.
				obstacleTreeNodesPtr = (KdTree.ObstacleTreeNode*)kdTreeToBuild.obstacleTreeNodes.GetUnsafePtr();
				node = obstacleTreeNodesPtr + nodeIndex;
				node->leftIndex = leftIndex;

				var rightIndex = BuildObstacleTreeRecursive(
					ref kdTreeToBuild,
					(int*)rightObstacles.GetUnsafePtr(),
					rightObstacles.Length);

				// NewObstacleTreeNode() in BuildObstacleTreeRecursive() will cause obstacleTreeNodes change
				// so the old obstacleTreeNodesPtr value became invalid and we have to assign again.
				obstacleTreeNodesPtr = (KdTree.ObstacleTreeNode*)kdTreeToBuild.obstacleTreeNodes.GetUnsafePtr();
				node = obstacleTreeNodesPtr + nodeIndex;
				node->rightIndex = rightIndex;

				leftObstacles.Dispose();
				rightObstacles.Dispose();

				return nodeIndex;
			}
		}

		private void Dispose(bool disposing)
		{
			if(!disposedValue)
			{
				if(disposing)
				{
					// Managed state
					Clear();
				}

				agents.Dispose();
				obstacles.Dispose();

				kdTree.Dispose();

				agentIndexLookup.Dispose();
				obstacleIndexLookup.Dispose();

				// Unmanaged resources
				disposedValue = true;
			}
		}

		private unsafe Agent* NewAgent()
		{
			var newIndex = agents.Length;
			var agentId = ++sid;
			var agent = new Agent(agentId);

			agents.Add(agent);
			agentIndexLookup[agentId] = newIndex;

			agentTreeDirty = true;

			return agents.GetUnsafePtr() + newIndex;
		}

		[BurstCompile]
		private struct BuildJob : IJob
		{
			private KdTree kdTree;
			[ReadOnly]
			private NativeArray<Agent>.ReadOnly agents;

			public BuildJob(KdTree kdTree, NativeArray<Agent>.ReadOnly agents)
				: this()
			{
				this.kdTree = kdTree;
				this.agents = agents;
			}

			public unsafe void Execute()
			{
				BuildAgentTree(ref kdTree, (Agent*)agents.GetUnsafeReadOnlyPtr(), agents.Length);
			}
		}

		[BurstCompile]
		private struct ComputeJob : IJobParallelFor
		{
			private readonly float timeStep;
			[WriteOnly]
			private NativeArray<float2> agentResult;
			[ReadOnly]
			private NativeArray<Agent>.ReadOnly agents;
			[ReadOnly]
			private NativeArray<Obstacle>.ReadOnly obstacles;
			[ReadOnly]
			private readonly KdTree.ReadOnly kdTree;

			public ComputeJob(
				NativeArray<Agent>.ReadOnly agents,
				NativeArray<Obstacle>.ReadOnly obstacles,
				KdTree.ReadOnly kdTree,
				float timeStep,
				NativeArray<float2> agentResult)
				: this()
			{
				this.agents = agents;
				this.obstacles = obstacles;
				this.kdTree = kdTree;
				this.timeStep = timeStep;
				this.agentResult = agentResult;
			}

			public unsafe void Execute(int index)
			{
				var agentNeighbors = new UnsafeList<Agent.Pair>(8, Allocator.Temp);
				var obstacleNeighbors = new UnsafeList<Agent.Pair>(8, Allocator.Temp);

				var agentsPtr = (Agent*)agents.GetUnsafeReadOnlyPtr();
				var obstaclesPtr = (Obstacle*)obstacles.GetUnsafeReadOnlyPtr();
				var agentsLength = agents.Length;
				var obstaclesLength = obstacles.Length;
				var agent = agentsPtr + index;

				agent->ComputeNeighbors(
					in index,
					in kdTree,
					agentsPtr,
					agentsLength,
					obstaclesPtr,
					obstaclesLength,
					ref agentNeighbors,
					ref obstacleNeighbors);
				agent->ComputeNewVelocity(
					timeStep,
					agentsPtr,
					agentsLength,
					obstaclesPtr,
					obstaclesLength,
					ref agentNeighbors,
					ref obstacleNeighbors);
				agentResult[index] = agent->newVelocity;

				agentNeighbors.Dispose();
				obstacleNeighbors.Dispose();
			}
		}

		[BurstCompile]
		private struct UpdateJob : IJobParallelFor
		{
			private readonly float timeStep;
			[ReadOnly]
			private NativeArray<float2> agentResult;
			private NativeArray<Agent> agents;

			public UpdateJob(
				NativeArray<Agent> agents,
				float timeStep,
				NativeArray<float2> agentResult)
				: this()
			{
				this.agents = agents;
				this.timeStep = timeStep;
				this.agentResult = agentResult;
			}

			public void Execute(int index)
			{
				var agent = agents[index];
				agent.newVelocity = agentResult[index];
				agent.Update(timeStep);
				agents[index] = agent;
			}
		}
	}
}
