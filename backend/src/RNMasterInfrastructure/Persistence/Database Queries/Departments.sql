IF OBJECT_ID(N'Departments', N'U') IS NULL
BEGIN
    CREATE TABLE Departments
    (
        DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
        DepartmentName VARCHAR(150) NOT NULL,
        DepartmentCode VARCHAR(50) NOT NULL,
        Description VARCHAR(500) NULL,

        Status VARCHAR(30) NOT NULL DEFAULT 'Active',
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,

        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy VARCHAR(450) NULL,
        UpdatedAt DATETIME NULL,
        UpdatedBy VARCHAR(450) NULL
    );
END;