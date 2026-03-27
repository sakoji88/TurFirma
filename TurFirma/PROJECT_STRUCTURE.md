# TurFirma WPF (MVVM)

## Структура проекта

- `App.xaml`, `MainWindow.xaml` — запуск и главное окно с навигацией по вкладкам.
- `Models/` — сущности предметной области (`User`, `Tour`, `Booking`, `Payment`, и т.д.).
- `Data/TourFirmaDbContext.cs` — EF Core контекст + подключение к `(localdb)\\MSSQLLocalDB`.
- `Services/` — бизнес-логика:
  - `AuthService` — регистрация/авторизация.
  - `TourService` — поиск туров и расчет доступных мест.
  - `BookingAppService` — корзина, расчет итоговой стоимости, создание брони, оплата.
  - `ManagerService` — подтверждение бронирований, назначение гида/транспорта.
- `ViewModels/` — MVVM слой для экранов:
  - `AuthViewModel`, `TourCatalogViewModel`, `CartViewModel`, `ProfileViewModel`, `AdminViewModel`, `MainViewModel`.
- `Infrastructure/` — базовые классы MVVM (`ObservableObject`, `RelayCommand`).
- `Scripts/Create_TurFirma_LocalDB.sql` — полный SQL-скрипт создания БД с ограничениями, триггерами и тестовыми данными.

## Подключение к LocalDB
Строка подключения в `TourFirmaDbContext`:

`Server=(localdb)\\MSSQLLocalDB;Database=TurFirmaDb;Trusted_Connection=True;TrustServerCertificate=True;`

## Запуск
1. Выполнить SQL-скрипт `Scripts/Create_TurFirma_LocalDB.sql` в SSMS.
2. Собрать и запустить WPF-приложение.
3. Для входа использовать тестовые аккаунты:
   - Клиент: `client@tour.local` / `12345`
   - Менеджер: `manager@tour.local` / `12345`
