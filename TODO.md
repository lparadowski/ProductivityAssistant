# TODO: Improve File Discovery with Claude-Based Selection

## Problem

The current topic/entity extraction approach doesn't always lead to meaningful files being retrieved. Pattern matching based on extracted keywords can miss relevant files or return irrelevant ones.

## Proposed Solution

Replace pattern-based file discovery with Claude-based file selection:

1. Send the filtered file hierarchy (paths only, no content) along with the ticket description to Claude
2. Claude determines which files are relevant based on semantic understanding of the ticket and file/folder naming
3. Retrieve content for those selected files
4. Continue with existing analysis/generation flow

## Current Flow

```
Ticket → Claude extracts topics → Pattern matching finds files → File contents sent to Claude
```

## Proposed Flow

```
Ticket + File list → Claude selects relevant files → File contents sent to Claude
```

## Benefits

- **More accurate selection**: Claude uses semantic reasoning, not just keyword matching
- **Removes weak link**: No intermediate topic extraction that can produce vague/incorrect results
- **Smaller first payload**: Just file paths, not content
- **Leverages naming conventions**: Claude can infer relevance from folder structure and file names
- **Simpler code**: Less parsing of extraction results needed
- **Unified approach**: Same file selection for both Investigation and Coding workflows (see below)

## Implementation Details

### Current Flow (Code)

```csharp
// In CodingService / InvestigationService
var topics = await claudeApiService.ExtractCodeChangeTopicsAsync(title, description);
var codeSamples = await codeSampleLocator.FindRelevantCodeAsync(topics, codebasePath);
```

### Proposed Flow (Code)

```csharp
// In CodingService / InvestigationService
var fileList = await codeSampleLocator.GetFilteredFileListAsync(codebasePath);
var relevantFiles = await claudeApiService.DetermineRelevantFilesAsync(title, description, fileList);
var codeSamples = await codeSampleLocator.GetCodeSamplesForFilesAsync(relevantFiles, codebasePath);
```

### New Methods Required

| Interface | Method | Purpose |
|-----------|--------|---------|
| `ICodeSampleLocator` | `GetFilteredFileListAsync(codebasePath)` | Returns `string[]` of filtered file paths (no content) |
| `ICodeSampleLocator` | `GetCodeSamplesForFilesAsync(filePaths[], codebasePath)` | Returns `CodeSample[]` for specific paths |
| `IClaudeApiService` | `DetermineRelevantFilesAsync(title, desc, fileList)` | Returns `string[]` of files Claude deems relevant |

### Methods Replaced

| Interface | Old Method | Notes |
|-----------|------------|-------|
| `IClaudeApiService` | `ExtractCodeChangeTopicsAsync` | Replaced by `DetermineRelevantFilesAsync` |
| `IClaudeApiService` | `ExtractTopicsAsync` | Replaced by `DetermineRelevantFilesAsync` |

### Models to Remove

The current implementation has separate topic models for investigation vs coding:

| Model | Purpose | Why It Existed |
|-------|---------|----------------|
| `InvestigationTopics` | Technologies, Actions, FilePatterns | Investigations focus on how technologies are used |
| `CodeChangeTopics` | Entities, Operations, Layers, FilePatterns | Code changes focus on entity files across layers |

**With the new approach, both models become unnecessary.** Claude sees the full file list and infers from the ticket context whether it's an investigation (broader technology search) or a code change (specific entity files). The differentiation moves to the prompt, not the data model.

**Files to delete:**
- `src/Application/Model/InvestigationTopics.cs`
- `src/Application/Model/CodeChangeTopics.cs`

**Prompts to remove from `PromptHelper.cs`:**
- `BuildTopicExtractionPrompt`
- `BuildCodeChangeTopicExtractionPrompt`

### Implementation Notes

- `GetFilteredFileListAsync`: Reuse existing directory traversal and blacklist filtering from `CodeSampleLocator`
- `GetCodeSamplesForFilesAsync`: Simple file read for given paths, return as `CodeSample[]`
- `DetermineRelevantFilesAsync`: New Claude prompt asking it to select relevant files (limit to ~10 files)
- Existing `FindRelevantCodeAsync(topics, codebasePath)` can remain for backwards compatibility or be removed

## Files to Modify

| File | Changes |
|------|---------|
| `ICodeSampleLocator.cs` | Add `GetFilteredFileListAsync` and `GetCodeSamplesForFilesAsync` |
| `CodeSampleLocator.cs` | Implement both new methods |
| `IClaudeApiService.cs` | Add `DetermineRelevantFilesAsync`, remove `ExtractTopicsAsync` and `ExtractCodeChangeTopicsAsync` |
| `ClaudeApiService.cs` | Implement new method, remove old extraction methods |
| `PromptHelper.cs` | Add `BuildFileSelectionPrompt`, remove `BuildTopicExtractionPrompt` and `BuildCodeChangeTopicExtractionPrompt` |
| `CodingService.cs` | Update `PerformCodeChange` to use new flow |
| `InvestigationService.cs` | Update `InvestigateAsync` to use new flow |

## Files to Delete

| File | Reason |
|------|--------|
| `InvestigationTopics.cs` | No longer needed - replaced by simple `string[]` of file paths |
| `CodeChangeTopics.cs` | No longer needed - replaced by simple `string[]` of file paths |
| `TopicsExtension.cs` | Remove `ParseTopicExtraction` and `ParseCodeChangeTopics` methods (keep `ParseCodeChangeSuggestions` if still used) |

---

## Future Approach: Embedding-Based Retrieval

A more advanced alternative using vector embeddings for semantic code search.

### Concept

Instead of Claude picking from a file list, Claude formulates semantic queries and retrieves matching code chunks via tooling.

### Flow

```
Ticket → Claude + Search Tool → Claude queries as needed → Relevant chunks returned → Analysis/Generation
```

### How It Would Work

1. **Setup (one-time + sync)**
   - Embed the codebase with Voyage (or similar)
   - Store in vector DB (Pinecone, Qdrant, Chroma, etc.)
   - Keep embeddings in sync as code changes

2. **At runtime**
   - Claude receives the ticket + a `search_codebase` tool
   - Claude decides what to search for: "order validation logic", "customer repository interface", etc.
   - Tool returns top-k semantically similar chunks
   - Claude can search multiple times, refining queries based on results
   - Once satisfied, Claude proceeds with analysis/generation

### Example Tool Definition

```json
{
  "name": "search_codebase",
  "description": "Search the codebase for semantically relevant code",
  "parameters": {
    "query": "natural language description of what you're looking for",
    "top_k": "number of results (default 5)"
  }
}
```

### Comparison with File List Approach

| Aspect | File List Approach | Embedding Approach |
|--------|-------------------|-------------------|
| Selection method | Claude picks by path/name | Claude queries by meaning |
| Granularity | Whole files | Chunks (functions, classes) |
| Large codebases | List gets unwieldy | Scales well |
| Setup complexity | Low | Higher (vector DB, sync pipeline) |
| Finds renamed/refactored code | No | Yes (semantic similarity) |
| Iterative refinement | Would need multiple calls | Natural with tooling |

### When to Consider This Approach

- Very large codebases where file lists are impractical
- When you need function/method-level granularity
- When naming conventions are inconsistent
- When you want Claude to explore iteratively

### Hybrid Option

Combine both approaches:
1. Send file list for high-level orientation
2. Provide embedding search tool for deep dives into specific functionality
