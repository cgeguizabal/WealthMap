# WealthMap — Project Rules for Claude Code

## Commit rules
- Never add Co-Authored-By trailers or any AI attribution to commit messages.
- Author commits as the developer only.
- Commit after each coherent step, conventional style (feat:, fix:, refactor:).
- NEVER run `git push`.

## Environment
- Windows, PowerShell. All commands single-line — no bash `\` continuations.
- EF commands run from repo root (WealthMap_Back-End):
  dotnet ef migrations add NAME --project src/WealthMap.Infrastructure --startup-project src/WealthMap.Api
- Corporate VPN blocks Neon (port 5432). Migrations can be generated offline;
  `database update` may fail — that's expected, not a bug to chase.

## Architecture (non-negotiable)
- Clean Architecture: Domain → Application → Infrastructure → Api. Never violate the dependency rule.
- Hand-built CQRS mediator in Application/Common/Messaging. Do NOT install MediatR or AutoMapper.
- Rich domain entities: private setters, validating constructors, business methods throwing DomainException.
- Money value object for all monetary fields. Store facts, compute conclusions.
- Every repository query is user-scoped. "Not yours" returns 404, not 403.
- Multi-entity writes go through IUnitOfWork.ExecuteInTransactionAsync.
- snake_case DB naming via UseSnakeCaseNamingConvention().

## Working style
- Stop after each module. Output a Postman test checklist and wait for confirmation before continuing.
- Do not create a .claude folder or Claude-specific settings files in the repo.
- Concise explanations. Recommend, don't just present options.