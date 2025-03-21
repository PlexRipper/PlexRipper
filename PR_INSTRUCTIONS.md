# Pull Request Submission Instructions

## Creating a Pull Request from Your Fork

Now that we've successfully fixed the Torznab implementation issues and pushed the changes to your fork, you can create a pull request to the upstream repository. Follow these steps:

1. **Go to GitHub**: Visit the original PlexRipper repository at https://github.com/PlexRipper/PlexRipper

2. **Click "Pull requests"**: Navigate to the "Pull requests" tab in the repository.

3. **Click "New pull request"**: This will start the PR creation process.

4. **Click "Compare across forks"**: This allows you to create a PR from your fork.

5. **Select the repositories and branches**:
   - Base repository: `PlexRipper/PlexRipper`
   - Base branch: `main` (or the appropriate target branch)
   - Head repository: `baxterblk/PlexRipper`
   - Compare branch: `feature/torznab-integration`

6. **Click "Create pull request"**

7. **Fill in the PR details**:
   - **Title**: "Fix: Resolve Torznab implementation issues and improve documentation"
   - **Description**: Use the content from the PR_DESCRIPTION.md or PR_DESCRIPTION_SIMPLE.md file depending on how detailed you want the description to be.

8. **Submit the PR**: Click "Create pull request" to submit.

## What We Fixed

1. **Missing Methods**:
   - Added the `GetDataFolderLocation` method to the `IConfigManager` interface
   - Implemented this method in the `ConfigManager` class to return the `ConfigDirectory`

2. **API Implementation Issues**:
   - Fixed FastEndpoints API attribute syntax in Torznab endpoints
   - Changed from `QueryParam(BindFrom = "x")` to `QueryParam, BindFrom("x")`

3. **Property Reference Fixes**:
   - Changed from `MediaType` to the correct `Type` property in `TorznabSearchEndpoint.cs`
   - Updated `AddedAt` handling to properly use the required property

4. **Formatting Issues**:
   - Fixed formatting in multiple files to ensure consistent code style
   - Removed extra whitespace and BOM characters
   - Fixed indentation issues

5. **Documentation**:
   - Added comprehensive documentation in TORZNAB_BUGFIX_DOCUMENTATION.md
   - Updated README.md to highlight the Torznab feature

## Testing

The implementation has been tested with:
- Docker build completed successfully
- Verified functionality with the fixed issues

## Helpful Links for PR Creation

- [GitHub's guide to creating a PR from a fork](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/creating-a-pull-request-from-a-fork)
- [Writing effective PR descriptions](https://github.blog/2015-01-21-how-to-write-the-perfect-pull-request/)
