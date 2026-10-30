# Лабораторна робота № 1 — електронна таблиця (варіант 53)

Операції варіанта: **1, 6, 7, 8, 10**
- `+`, `-`, `*`, `/` — бінарні операції (ділення цілочисельне);
- `max(x, y)`, `min(x, y)`;
- `mmax(x1, …, xN)`, `mmin(x1, …, xN)`, N ≥ 1;
- `=`, `<`, `>`;
- `not`.

Цілі числа довільної довжини (`BigInteger`), посилання на клітинки `A1`, `B12`, `AA3`, круглі дужки.
Вираз логічний: 0 — ХИБНІСТЬ, інше число — ІСТИНА; ІСТИНА = 1, ХИБНІСТЬ = 0. Порожня клітинка = 0.

Пріоритет (від найвищого): дужки і функції → `*` `/` → `+` `-` → `=` `<` `>` → `not`.

## Структура

| Папка | Вміст |
|---|---|
| `SpreadsheetEngine` | обчислювач виразів: `Lexer.cs`, `Parser.cs`, `Ast.cs`, `Value.cs`, `CellAddress.cs`, `Spreadsheet.cs`, `Errors.cs` |
| `SpreadsheetEngine.Tests` | unit-тести xUnit |
| `SpreadsheetApp` | інтерфейс .NET MAUI |
| `docs` | UML-діаграми (PNG) |

## Запуск

```bash
dotnet test SpreadsheetEngine.Tests
cd SpreadsheetApp
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```
На Mac: `dotnet build -t:Run -f net10.0-maccatalyst` або F5 у Visual Studio.