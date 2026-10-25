# Contributing

Thanks for your interest in PintaNumberBadges!

## Reporting bugs

Please use the [GitHub issue tracker](https://github.com/dengzhipengyazhi/pinta-number-badges/issues)
and include:

- Pinta version (e.g. 3.1.2) and operating system,
- the add-in version,
- steps to reproduce, expected and actual behavior,
- a screenshot if possible.

Security issues: please **do not open a public issue**, see [SECURITY.md](SECURITY.md).

## Building

Requirements:

- .NET 8 SDK
- Pinta 3.1.x source (used as the compile-time reference)

Fetch the Pinta source and build:

```powershell
pwsh ./scripts/get-pinta-source.ps1
dotnet build -c Release
```

Build and produce a `.mpack` (requires `Mono.Addins.UtilTool`):

```powershell
.\build.ps1 -Pack
```

The add-in output is `bin/Release/net8.0/PintaNumberBadges.dll`; the installable
package is `package/PintaNumberBadges.<version>.mpack`.

## Pull requests

- Keep changes focused; do not reformat unrelated code.
- Match the existing code style (tabs, `Nullable` enabled, file headers with
  the MIT notice).
- Update `CHANGELOG.md` for user-visible changes.
- If you change the add-in version, update both `.addin.xml` and the
  `Version` property in `PintaNumberBadges.csproj`.
- Test by installing the built `.mpack` in Pinta 3.1.x and stamping a few
  badges (filled and hand-drawn), then undoing them.

## Licensing of contributions

By submitting a pull request you agree to license your contribution under the
MIT license (see [LICENSE](LICENSE)).

## Upstreaming to Pinta

This is an independent third-party add-in. The Pinta project has a
[contribution policy](https://github.com/PintaProject/Pinta/blob/master/patch-guidelines.md)
that requires code to be authored by a human contributor and explicitly
disallows AI-generated code for contributions to Pinta itself. If you plan to
submit this add-in to the Pinta community add-in repository, please make sure
you comply with that policy first.

The upstream Pinta project is not affiliated with this add-in.
