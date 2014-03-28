CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                   NVARCHAR (128) NOT NULL,
    [Email]                NVARCHAR (256) NULL,
    [EmailConfirmed]       BIT            NOT NULL,
    [PasswordHash]         NVARCHAR (MAX) NULL,
    [SecurityStamp]        NVARCHAR (MAX) NULL,
    [PhoneNumber]          NVARCHAR (MAX) NULL,
    [PhoneNumberConfirmed] BIT            NOT NULL,
    [TwoFactorEnabled]     BIT            NOT NULL,
    [LockoutEndDateUtc]    DATETIME       NULL,
    [LockoutEnabled]       BIT            CONSTRAINT [DF_AspNetUsers_LockoutEnabled] DEFAULT ((0)) NOT NULL,
    [AccessFailedCount]    INT            CONSTRAINT [DF_AspNetUsers_AccessFailedCount] DEFAULT ((0)) NOT NULL,
    [UserName]             NVARCHAR (256) NOT NULL,
    CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AspNetUsers_UserName]
    ON [dbo].[AspNetUsers]([UserName] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_AspNetUsers_Email]
    ON [dbo].[AspNetUsers]([Email] ASC);

