#!/bin/bash

# Define the new hooks directory
HOOKS_DIR=".githooks"

# Check if the repository is already initialized
if [ ! -d .git ]; then
    echo "Error: This directory is not a Git repository."
    exit 1
fi

# Create the new hooks directory if it doesn't exist
if [ ! -d "$HOOKS_DIR" ]; then
    mkdir "$HOOKS_DIR"
    echo "Created new hooks directory: $HOOKS_DIR"
fi

# Set the new hooks path
git config core.hooksPath "$HOOKS_DIR"

# Verify the change
NEW_PATH=$(git config core.hooksPath)
if [ "$NEW_PATH" = "$HOOKS_DIR" ]; then
    echo "Successfully set Git hooks path to: $HOOKS_DIR"
else
    echo "Failed to set Git hooks path."
    exit 1
fi


echo "Done! You can now add your hooks to the $HOOKS_DIR directory."