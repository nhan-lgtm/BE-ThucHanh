--New Database: MyStore
-- Bang User: quan ly ca tai khoan cua Khach hang va Admin
CREATE TABLE [dbo].[User] (
    [Username]     NVARCHAR (255) NOT NULL,
    [Password] NCHAR (50)     NOT NULL,
   [UserRole]     NCHAR (1) NOT NULL, -- A: admin, C: customer
    PRIMARY KEY CLUSTERED ([Username] ASC)
);

--Bang Customer: luu thong tin khach hang
CREATE TABLE [dbo].[Customer] (
    [CustomerID]          INT IDENTITY (1, 1) NOT NULL,
    [CustomerName]     NVARCHAR (MAX) NOT NULL,
    [CustomerPhone]    NVARCHAR (15)  NOT NULL,
    [CustomerEmail]      NVARCHAR (MAX) NULL,
    [CustomerAddress]  NVARCHAR (MAX) NULL,
    [Username]              NVARCHAR (255) NOT NULL,
    PRIMARY KEY CLUSTERED ([CustomerID] ASC),
    CONSTRAINT [FK_User_Customer] FOREIGN KEY ([Username]) REFERENCES [dbo].[User] ([Username])
);

--Bang Category: luu thong tin danh muc san pham
CREATE TABLE [dbo].[Category] (
    [CategoryID]            INT            IDENTITY (1, 1) NOT NULL,
    [CategoryName]      NVARCHAR (MAX) NOT NULL,
    PRIMARY KEY CLUSTERED ([CategoryID] ASC)
);
--Bang Product: luu thong tin san pham
CREATE TABLE [dbo].[Product] (
    [ProductID]              INT             IDENTITY (1, 1) NOT NULL,
    [CategoryID]            INT            NOT NULL,
    [ProductName]        NVARCHAR (MAX)  NOT NULL,
    [ProductDecription]  NVARCHAR (MAX)  NOT NULL,
    [ProductPrice]          DECIMAL (18, 2) NOT NULL,
    [ProductImage]        NVARCHAR (MAX)  NULL,
    PRIMARY KEY CLUSTERED ([ProductID] ASC),
    CONSTRAINT [FK_Pro_Category] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Category] ([CategoryID])
);

--Bang Order: luu thong tin don hang cua khach hang
CREATE TABLE [dbo].[Order] (
    [OrderID]                 INT            IDENTITY (1, 1) NOT NULL,
    [CustomerID]          INT            NOT NULL,
    [OrderDate]             DATE           NOT NULL,
    [TotalAmount]         DECIMAL (18, 2) NOT NULL,
    [PaymentStatus]     NVARCHAR (MAX) NULL,
    [AddressDelivery]   NVARCHAR (MAX) NOT NULL,
    PRIMARY KEY CLUSTERED ([OrderID] ASC),
    FOREIGN KEY ([CustomerID]) REFERENCES [dbo].[Customer] ([CustomerID])
);
--Bang OrderDetail: luu thong tin chi tiet cac san pham co trong don hang
CREATE TABLE [dbo].[OrderDetail] (
    [ID]        INT        IDENTITY (1, 1) NOT NULL,
    [ProductID] INT        NOT NULL,
    [OrderID]   INT        NOT NULL,
    [Quantity]  INT        NOT NULL,
    [UnitPrice] DECIMAL (18, 2) NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Product] ([ProductID]),
    FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Order] ([OrderID])
);

ALTER AUTHORIZATION ON DATABASE::MyStore TO sa;
ALTER TABLE [dbo].[User] ALTER COLUMN UserRole NVARCHAR(20) NOT NULL;
USE MyStore;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[User] WHERE Username = 'admin')
    INSERT INTO [dbo].[User] (Username, [Password], UserRole)
    VALUES ('admin', 'admin123', 'A');
ELSE
    UPDATE [dbo].[User]
    SET [Password] = 'admin123', UserRole = 'A'
    WHERE Username = 'admin';
GO

SELECT * FROM [dbo].[User];
UPDATE [dbo].[User] SET UserRole = 'C' WHERE UserRole = 'Customer';

SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Order' AND COLUMN_NAME = 'PaymentStatus';

UPDATE [dbo].[Order] SET PaymentStatus = N'Ch? x? l?' WHERE PaymentStatus = 'Pending';
UPDATE [dbo].[Order] SET PaymentStatus = N'Đ? thanh toán' WHERE PaymentStatus = 'Paid';
UPDATE [dbo].[Order] SET PaymentStatus = N'Đang giao' WHERE PaymentStatus = 'Shipped';
UPDATE [dbo].[Order] SET PaymentStatus = N'Hoàn thành' WHERE PaymentStatus = 'Completed';
UPDATE [dbo].[Order] SET PaymentStatus = N'Đ? h?y' WHERE PaymentStatus = 'Cancelled';

SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Order' AND COLUMN_NAME IN ('OrderDate', 'StatusUpdatedAt');

ALTER TABLE [dbo].[Order] ALTER COLUMN OrderDate DATETIME NOT NULL;