/*
  Enhanzer Assignment database schema.
  Optional safe cleanup for local development:
  DROP TABLE IF EXISTS dbo.Purchase_Bill_Items;
  DROP TABLE IF EXISTS dbo.Purchase_Bills;
  DROP TABLE IF EXISTS dbo.Location_Details;
*/

IF OBJECT_ID(N'dbo.Location_Details', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Location_Details
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Location_Details PRIMARY KEY,
        Location_Code NVARCHAR(50) NOT NULL,
        Location_Name NVARCHAR(200) NOT NULL,
        Created_At_Utc DATETIME2 NOT NULL,
        Updated_At_Utc DATETIME2 NOT NULL
    );
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_Location_Details_Location_Code'
      AND object_id = OBJECT_ID(N'dbo.Location_Details')
)
BEGIN
    CREATE UNIQUE INDEX UX_Location_Details_Location_Code
        ON dbo.Location_Details(Location_Code);
END;

IF OBJECT_ID(N'dbo.Purchase_Bills', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Purchase_Bills
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Purchase_Bills PRIMARY KEY,
        User_Email NVARCHAR(256) NOT NULL,
        Total_Items INT NOT NULL,
        Total_Quantity DECIMAL(18,2) NOT NULL,
        Total_Cost DECIMAL(18,2) NOT NULL,
        Total_Selling DECIMAL(18,2) NOT NULL,
        Created_At_Utc DATETIME2 NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.Purchase_Bill_Items', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Purchase_Bill_Items
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Purchase_Bill_Items PRIMARY KEY,
        Purchase_Bill_Id UNIQUEIDENTIFIER NOT NULL,
        Item_Name NVARCHAR(100) NOT NULL,
        Location_Code NVARCHAR(50) NOT NULL,
        Location_Name NVARCHAR(200) NOT NULL,
        Standard_Cost DECIMAL(18,2) NOT NULL,
        Standard_Price DECIMAL(18,2) NOT NULL,
        Quantity DECIMAL(18,2) NOT NULL,
        Discount_Percentage DECIMAL(5,2) NOT NULL,
        Total_Cost DECIMAL(18,2) NOT NULL,
        Total_Selling DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_Purchase_Bill_Items_Purchase_Bills
            FOREIGN KEY (Purchase_Bill_Id)
            REFERENCES dbo.Purchase_Bills(Id)
            ON DELETE CASCADE
    );
END;
