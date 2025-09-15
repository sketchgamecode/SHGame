# Scripts Directory

This directory contains all C# scripts for the SHGame Unity project.

## Organization

- **Core/**: Core game systems (Player, GameManager, etc.)
- **UI/**: User interface scripts
- **Audio/**: Audio management scripts
- **Utilities/**: Helper classes and utilities
- **ScriptableObjects/**: Data containers and configurations

## Coding Standards

Follow the guidelines specified in the [Technical Design Document](../../docs/technical/TDD.md):
- Use PascalCase for class and method names
- Use camelCase for variable names
- Document public APIs with XML comments
- Keep methods under 20 lines when possible

## AI Review Points

Scripts in this directory will be validated against:
- Architecture patterns defined in TDD
- Performance requirements
- Unity best practices
- Code quality standards