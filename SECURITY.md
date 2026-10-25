# Security policy

## Scope

PintaNumberBadges is a local add-in for [Pinta](https://www.pinta-project.com/).
It runs inside Pinta and does not include any networking code.

- No telemetry, analytics or tracking.
- No network requests at runtime.
- No credentials, API keys, tokens or personal data are used or stored.
- The only files it touches are Pinta's own settings storage
  (`%APPDATA%\Pinta\settings.xml` on Windows) for tool preferences, and the
  document's scratch "tool layer" while a preview is shown.

## Reporting a vulnerability

If you find a security issue (for example a way the add-in could read files it
should not, or a crash triggered by malicious input), please **do not open a
public issue**. Use GitHub's private vulnerability reporting:

- <https://github.com/dengzhipengyazhi/pinta-number-badges/security/advisories/new>

Or contact the maintainer through the GitHub profile
[@dengzhipengyazhi](https://github.com/dengzhipengyazhi).

Please include:

- affected version / commit,
- Pinta version and operating system,
- steps to reproduce,
- the expected and actual behavior.
