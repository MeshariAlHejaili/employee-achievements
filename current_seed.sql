IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO
 
BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250712222757_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [ProfilePicture] nvarchar(200) NULL,
        [Department] nvarchar(100) NULL,
        [Position] nvarchar(100) NULL,
        [Email] nvarchar(100) NOT NULL,
        [Password] nvarchar(100) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250712222757_InitialCreate'
)
BEGIN
    CREATE TABLE [Achievements] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(2000) NOT NULL,
        [Date] datetime2 NOT NULL,
        [OwnerId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Achievements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Achievements_Users_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250712222757_InitialCreate'
)
BEGIN
    CREATE TABLE [Comments] (
        [Id] int NOT NULL IDENTITY,
        [Content] nvarchar(1000) NOT NULL,
        [Date] datetime2 NOT NULL,
        [UserId] int NOT NULL,
        [AchievementId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Comments_Achievements_AchievementId] FOREIGN KEY ([AchievementId]) REFERENCES [Achievements] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Comments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250712222757_InitialCreate'
)
BEGIN
    CREATE TABLE [Likes] (
        [Id] int NOT NULL IDENTITY,
        [Date] datetime2 NOT NULL,
        [UserId] int NOT NULL,
        [AchievementId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Likes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Likes_Achievements_AchievementId] FOREIGN KEY ([AchievementId]) REFERENCES [Achievements] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Likes_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250712222757_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Department', N'Email', N'Name', N'Password', N'Position', N'ProfilePicture') AND [object_id] = OBJECT_ID(N'[Users]'))
        SET IDENTITY_INSERT [Users] ON;
    EXEC(N'INSERT INTO [Users] ([Id], [CreatedAt], [Department], [Email], [Name], [Password], [Position], [ProfilePicture])
    VALUES (1, ''2024-06-01T08:00:00.0000000'', N''تطوير البرمجيات'', N''mashari@amana.com'', N''مشاري الحربي'', N''123456'', N''مطور برمجيات'', NULL),
    (2, ''2024-06-01T08:00:00.0000000'', N''إدارة المنتج'', N''sara@amana.com'', N''سارة أحمد'', N''123456'', N''مدير منتج'', NULL),
    (3, ''2024-06-01T08:00:00.0000000'', N''تطوير البرمجيات'', N''ahmed@amana.com'', N''أحمد محمد'', N''123456'', N''مطور خلفي'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Department', N'Email', N'Name', N'Password', N'Position', N'ProfilePicture') AND [object_id] = OBJECT_ID(N'[Users]'))
        SET IDENTITY_INSERT [Users] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250712222757_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Date', N'Description', N'OwnerId', N'Title') AND [object_id] = OBJECT_ID(N'[Achievements]'))
        SET IDENTITY_INSERT [Achievements] ON;
    EXEC(N'INSERT INTO [Achievements] ([Id], [CreatedAt], [Date], [Description], [OwnerId], [Title])
    VALUES (1, ''2024-06-02T09:00:00.0000000'', ''2024-06-25T00:00:00.0000000'', N''إعادة تصميم كاملة لتدفق تأهيل المستخدمين لمشروع نيوسواك، مما أدى إلى تحسين تجربة المستخدم بنسبة 40% وتقليل وقت التدريب إلى النصف.'', 1, N''مشروع نيوسواك''),
    (2, ''2024-06-02T09:00:00.0000000'', ''2024-06-20T00:00:00.0000000'', N''تطوير نظام تقارير جديد للإدارة يوفر رؤى شاملة عن أداء الفريق ومؤشرات الأداء الرئيسية.'', 2, N''تطوير نظام التقارير''),
    (3, ''2024-06-02T09:00:00.0000000'', ''2024-06-18T00:00:00.0000000'', N''تحسين أداء النظام الأساسي بنسبة 60% من خلال تحسين قاعدة البيانات وتحسين الخوارزميات.'', 3, N''تحسين أداء النظام'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Date', N'Description', N'OwnerId', N'Title') AND [object_id] = OBJECT_ID(N'[Achievements]'))
        SET IDENTITY_INSERT [Achievements] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250712222757_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AchievementId', N'Content', N'CreatedAt', N'Date', N'UserId') AND [object_id] = OBJECT_ID(N'[Comments]'))
        SET IDENTITY_INSERT [Comments] ON;
    EXEC(N'INSERT INTO [Comments] ([Id], [AchievementId], [Content], [CreatedAt], [Date], [UserId])
    VALUES (1, 1, N''عمل رائع! هذا سيساعد كثيراً في تحسين تجربة المستخدمين.'', ''2024-06-03T10:00:00.0000000'', ''2024-06-25T10:30:00.0000000'', 2),
    (2, 1, N''أحسنت! هذا التطوير سيحدث فرقاً كبيراً.'', ''2024-06-03T10:00:00.0000000'', ''2024-06-25T11:15:00.0000000'', 3),
    (3, 2, N''ممتاز! هذا النظام سيوفر لنا رؤية واضحة عن الأداء.'', ''2024-06-03T10:00:00.0000000'', ''2024-06-20T14:20:00.0000000'', 1)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AchievementId', N'Content', N'CreatedAt', N'Date', N'UserId') AND [object_id] = OBJECT_ID(N'[Comments]'))
        SET IDENTITY_INSERT [Comments] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250712222757_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AchievementId', N'CreatedAt', N'Date', N'UserId') AND [object_id] = OBJECT_ID(N'[Likes]'))
        SET IDENTITY_INSERT [Likes] ON;
    EXEC(N'INSERT INTO [Likes] ([Id], [AchievementId], [CreatedAt], [Date], [UserId])
    VALUES (1, 1, ''2024-06-04T11:00:00.0000000'', ''2024-06-25T09:00:00.0000000'', 2),
    (2, 1, ''2024-06-04T11:00:00.0000000'', ''2024-06-25T09:30:00.0000000'', 3),
    (3, 2, ''2024-06-04T11:00:00.0000000'', ''2024-06-20T10:00:00.0000000'', 1)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AchievementId', N'CreatedAt', N'Date', N'UserId') AND [object_id] = OBJECT_ID(N'[Likes]'))
        SET IDENTITY_INSERT [Likes] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250712222757_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Achievements_OwnerId] ON [Achievements] ([OwnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250712222757_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Comments_AchievementId] ON [Comments] ([AchievementId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250712222757_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Comments_UserId] ON [Comments] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250712222757_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Likes_AchievementId] ON [Likes] ([AchievementId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250712222757_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Likes_UserId_AchievementId] ON [Likes] ([UserId], [AchievementId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250712222757_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250712222757_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250712222757_InitialCreate', N'9.0.6');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Department');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [Users] DROP COLUMN [Department];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    ALTER TABLE [Users] ADD [DepartmentId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    ALTER TABLE [Users] ADD [ManagerId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    ALTER TABLE [Achievements] ADD [Status] nvarchar(20) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    CREATE TABLE [Departments] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Departments] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    CREATE TABLE [Managers] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [DepartmentId] int NOT NULL,
        CONSTRAINT [PK_Managers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Managers_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Managers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    EXEC(N'UPDATE [Achievements] SET [Status] = N''Pending''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    EXEC(N'UPDATE [Achievements] SET [Status] = N''Pending''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    EXEC(N'UPDATE [Achievements] SET [Status] = N''Pending''
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Departments]'))
        SET IDENTITY_INSERT [Departments] ON;
    EXEC(N'INSERT INTO [Departments] ([Id], [Name])
    VALUES (1, N''تطوير البرمجيات''),
    (2, N''إدارة المنتج'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Departments]'))
        SET IDENTITY_INSERT [Departments] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    EXEC(N'UPDATE [Users] SET [DepartmentId] = 1, [ManagerId] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    EXEC(N'UPDATE [Users] SET [DepartmentId] = 2, [ManagerId] = NULL
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    EXEC(N'UPDATE [Users] SET [DepartmentId] = 1, [ManagerId] = NULL
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'DepartmentId', N'Email', N'ManagerId', N'Name', N'Password', N'Position', N'ProfilePicture') AND [object_id] = OBJECT_ID(N'[Users]'))
        SET IDENTITY_INSERT [Users] ON;
    EXEC(N'INSERT INTO [Users] ([Id], [CreatedAt], [DepartmentId], [Email], [ManagerId], [Name], [Password], [Position], [ProfilePicture])
    VALUES (10, ''2024-07-17T08:00:00.0000000'', 1, N''manager1@amana.com'', NULL, N''مدير البرمجيات'', N''test123'', N''مدير قسم البرمجيات'', NULL),
    (11, ''2024-07-17T08:00:00.0000000'', 2, N''manager2@amana.com'', NULL, N''مدير المنتج'', N''test123'', N''مدير قسم المنتج'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'DepartmentId', N'Email', N'ManagerId', N'Name', N'Password', N'Position', N'ProfilePicture') AND [object_id] = OBJECT_ID(N'[Users]'))
        SET IDENTITY_INSERT [Users] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'DepartmentId', N'UserId') AND [object_id] = OBJECT_ID(N'[Managers]'))
        SET IDENTITY_INSERT [Managers] ON;
    EXEC(N'INSERT INTO [Managers] ([Id], [DepartmentId], [UserId])
    VALUES (1, 1, 10),
    (2, 2, 11)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'DepartmentId', N'UserId') AND [object_id] = OBJECT_ID(N'[Managers]'))
        SET IDENTITY_INSERT [Managers] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Users_DepartmentId] ON [Users] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Users_ManagerId] ON [Users] ([ManagerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Managers_DepartmentId] ON [Managers] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Managers_UserId] ON [Managers] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_Managers_ManagerId] FOREIGN KEY ([ManagerId]) REFERENCES [Managers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250720065942_InitialSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250720065942_InitialSchema', N'9.0.6');
END;

COMMIT;
GO

