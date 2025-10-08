# Productivity Assistant - AI-Powered Development Automation 

Proof of concept for an autonomous coding assistant that monitors Trello boards, analyzes tickets, generates code changes, and creates pull requests automatically.

## Overview

Productivity Assistant is a .NET background service that monitors Trello boards for new tickets, uses AI to understand requirements, locates relevant code, generates changes, and submits pull requests without human intervention.
This is build on the Claude API capabilities.

## Features

- **Trello Integration** - Monitors multiple boards for new tickets
- **AI-Powered Analysis** - Uses Claude (Anthropic) to understand requirements and generate code
- **Intelligent Code Discovery** - Automatically finds relevant code sections using pattern matching
- **Code Generation** - Creates, modifies, and deletes files following existing patterns
- **GitHub Automation** - Creates branches, commits changes, and opens pull requests
- **Multi-Codebase Support** - Handles multiple projects via label-based routing
- **Two Workflow Modes**:
  - **Investigation** - Analyzes complex technical questions and posts findings
  - **Simple** - Implements code changes and creates pull requests

## Architecture

The system uses a clean architecture approach. 

No Domain layer as it is not necessary at this stage (maybe in the future if this is coupled to a small DB to keep track of processed tickets). 
An API layer has been created to allow for a board processing endpoint but has not been implemented yet.

## Getting Started

### Prerequisites

- .NET 9 SDK
- Trello account with API access
- GitHub personal access token
- Anthropic API key

### Configuration

Configure using .NET user secrets:

```bash
dotnet user-secrets init --project src/API
```

**Required Settings:**

```json
{
  "ApplicationSettings": {
    "TrelloBoardIds": ["your-board-id"],
    "TrelloApiKey": "your-trello-api-key",
    "TrelloApiToken": "your-trello-api-token",
    "AnthropicApiKey": "your-anthropic-api-key",
    "GitHubToken": "your-github-token",
    "CodebaseMappings": {
      "ProjectA": "D:\\path\\to\\project-a"
    },
    "GitHubRepositories": {
      "ProjectA": {
        "Owner": "your-username",
        "Name": "repo-name"
      }
    },
    "ExecutionIntervalInSeconds": 3600,
    "MaxRetryDelayInSeconds": 300
  }
}
```

## Usage

### Label-Based Routing

Cards are routed using Trello labels:
- Add codebase label (e.g., `ProjectA`) to target the correct codebase
- Add workflow label (`Investigation` or `Simple`) to select the workflow type. This could be made configurable in a future version.

### Investigation Workflow

**Example Card:**
- Labels: `Investigation`, `ProjectA`
- Title: "Understand current caching strategy"
- Description: "Analyze how caching is implemented"

**Result:** AI posts detailed investigation report as a Trello comment.

### Coding Workflow

**Example Card:**
- Labels: `Simple`, `ProjectA`
- Title: "Add email validation to Customer entity"
- Description: "Email field needs validation"

**Result:** Creates branch, modifies code, commits, creates PR, posts link to Trello.

## How It Works

**1. Topic Extraction**
- AI analyzes ticket to extract entities, operations, layers, and file patterns

**2. Code Sample Location**
- Searches codebase using extracted patterns
- Finds and ranks relevant code sections

**3. AI Code Generation**
- Receives ticket description and code samples
- Generates changes following existing patterns
- Returns create/modify/delete operations

**4. Change Application**
- ChangeType 0: Create new files
- ChangeType 1: Modify existing files
- ChangeType 2: Delete files

**5. Git & PR Creation**
- Creates feature branch
- Commits all changes
- Pushes to GitHub
- Creates pull request
- Posts link to Trello

## Technology Stack

- .NET 9
- Anthropic Claude API
- Manatee.Trello (Trello API)
- LibGit2Sharp (Git operations)
- Clean Architecture

## License

MIT License

## Note

This is an experimental project demonstrating AI-assisted development. Always review generated code before merging to production.
This README has been partially generated with the Productivity Assistant.