IF OBJECT_ID(N'Tenants', N'U') IS NULL
BEGIN
    CREATE TABLE Tenants
    (
        TenantId INT IDENTITY(1,1) PRIMARY KEY,
        TenantName VARCHAR(150) NOT NULL,
        TenantCode VARCHAR(50) NOT NULL,
        Email VARCHAR(150) NULL,
        Phone VARCHAR(20) NULL,
        Status VARCHAR(30) NOT NULL DEFAULT 'Active',
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy VARCHAR(450) NULL,
        UpdatedAt DATETIME NULL,
        UpdatedBy VARCHAR(450) NULL
    )
END