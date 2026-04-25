# Contributing

## Branch strategy

- `main` is the stable integration branch.
- All regular changes must land in `main` through pull requests.
- Do not commit directly to `main`.
- Release automation runs from `main` if/when the repository adopts an automated release flow.
- Keep `main` linear. Prefer `Squash and merge` and disable plain merge commits in GitHub repository settings.

## Branch naming

Use short, descriptive branch names with this format:

```text
<type>/<short-description>
```

Recommended branch types:

- `feature/` for new capabilities
- `fix/` for bug fixes
- `refactor/` for internal code changes without behavior changes
- `docs/` for documentation-only changes
- `test/` for test-only changes
- `chore/` for maintenance or tooling changes
- `hotfix/` for urgent production fixes

Examples:

```text
feature/add-account-withdrawal
fix/deposit-event-persistence
docs/add-branching-guidelines
refactor/replace-static-event-publisher
test/account-aggregate-events
chore/migrate-to-dotnet-10
```

Guidelines:

- Use lowercase letters.
- Separate words with hyphens.
- Keep the name focused on one change.
- Do not add author or tool prefixes such as `codex/` unless the repository explicitly adopts that convention.
- Avoid generic names such as `test`, `changes`, or `new-branch`.

## Pull request target

- Open feature, fix, docs, refactor, test, and chore PRs against `main`.
- Do not open manual release PRs unless the repository maintainers explicitly decide to use a release PR flow.

## Pull request title

Prefer a concise Conventional Commits style title:

```text
type(scope): short summary
```

Examples:

```text
feat(api): expose account creation endpoint
fix(events): await event subscriber execution
docs(readme): document branch and PR workflow
refactor(domain): replace static event publisher
test(account): cover deposit event replay
chore(dotnet): migrate solution to dotnet 10
```

If no scope is useful, omit it:

```text
docs: clarify contribution flow
```

Guidelines:

- The title must describe the actual change in the PR.
- Do not use tool or author prefixes such as `[codex]`.
- Keep it specific enough that someone can understand the change from the PR list alone.
- Classify the title by consumer-facing impact, not by implementation technique.
- Use `feat:` when the API or example gains a new public capability.
- Use `fix:` when the change corrects observable behavior.
- Use `feat!:` or `fix!:` when the change is breaking for consumers, including removed endpoints, incompatible configuration, or changed message contracts.
- Avoid `refactor:` for consumer-visible API, configuration, behavior, or event contract changes.

Examples of refactor-shaped changes that should not be titled `refactor:`:

```text
feat: add withdrawal event flow
fix: validate deposit command before publishing events
feat!: rename account event routing keys
```

## Pull request checklist

Each PR should:

- describe the change clearly in the summary
- list the concrete changes included
- explain why the change is needed
- mark the affected scope
- describe how the change was validated
- include notes when there are tradeoffs, follow-ups, or limitations

When a PR template exists in `.github/pull_request_template.md`, use it for every PR.

## Suggested workflow

1. Create a branch from `main`.
2. Implement one focused change.
3. Run the relevant validation locally.
4. Open a PR to `main`.
5. Wait for review and CI before merge.
6. Merge to `main` with `Squash and merge`, keeping the PR title as the squash commit message.
7. Let the selected release process manage versioning and publishing.

## Optional repository enforcement

To reinforce this process in GitHub settings, maintainers can also:

- protect `main` and block direct pushes
- require at least one approving review
- require status checks before merge
- require branch to be up to date before merge
- require the PR template to be completed during review
- enable `Squash and merge`
- disable `Merge commit`
- default to PR title for squash merge commits
