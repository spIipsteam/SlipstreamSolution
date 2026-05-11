-- Создаём таблицу VendingMachines (если нет)
CREATE TABLE IF NOT EXISTS VendingMachines (
    Id INTEGER PRIMARY KEY,
    SerialNumber TEXT,
    InventoryNumber TEXT,
    Name TEXT,
    Model TEXT,
    Company TEXT,
    ModemId TEXT,
    Location TEXT,
    CommissioningDate TEXT,
    Status TEXT,
    TotalRevenue REAL
);

-- Добавляем тестовые автоматы
INSERT OR IGNORE INTO VendingMachines (Id, SerialNumber, InventoryNumber, Name, Model, Company, ModemId, Location, CommissioningDate, Status, TotalRevenue)
VALUES 
(1, 'SN001', 'INV001', 'Кофейный автомат', 'Jofemar T2', 'ООО Кофе', 'MODEM-001', 'ТЦ "Европа"', '2023-01-15', 'Работает', 150000),
(2, 'SN002', 'INV002', 'Снековый автомат', 'Azkoyen', 'ООО Снеки', 'MODEM-002', 'Бизнес-центр "Плаза"', '2023-03-20', 'Работает', 80000),
(3, 'SN003', 'INV003', 'Водный автомат', 'AquaPro', 'ООО Вода', 'MODEM-003', 'Ж/д вокзал', '2023-06-10', 'На обслуживании', 25000);

-- Создаём таблицу Sales
CREATE TABLE IF NOT EXISTS Sales (
    Id INTEGER PRIMARY KEY,
    SaleDate TEXT,
    Amount REAL,
    Quantity INTEGER
);

-- Добавляем тестовые продажи
INSERT OR IGNORE INTO Sales (Id, SaleDate, Amount, Quantity)
VALUES 
(1, date('now', '-1 days'), 15000, 45),
(2, date('now', '-2 days'), 23000, 67),
(3, date('now', '-3 days'), 18000, 52),
(4, date('now', '-4 days'), 21000, 60),
(5, date('now', '-5 days'), 19500, 55);

-- Создаём таблицу Users (если нет)
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY,
    FullName TEXT,
    Email TEXT UNIQUE,
    Role TEXT,
    PasswordHash TEXT
);

-- Добавляем пользователя
INSERT OR IGNORE INTO Users (Id, FullName, Email, Role, PasswordHash)
VALUES (1, 'Чернышов Руслан Мансурович', 'rushan.chernyshov@inbox.ru', 'Франчайзер', 'HFGDRET765');

-- Создаём таблицу News
CREATE TABLE IF NOT EXISTS News (
    Id INTEGER PRIMARY KEY,
    Title TEXT,
    Content TEXT,
    PublishDate TEXT
);

-- Добавляем новости
INSERT OR IGNORE INTO News (Id, Title, Content, PublishDate)
VALUES 
(1, 'Добро пожаловать', 'Система вендинга успешно запущена', date('now')),
(2, 'Акция', 'Скидка 20% на все напитки до конца месяца', date('now'));