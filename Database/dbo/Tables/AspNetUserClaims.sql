CREATE TABLE [dbo].[AspNetUserClaims] (
    [Id]         NVARCHAR (128) NOT NULL,
    [UserId]     NVARCHAR (128) NOT NULL,
    [ClaimType]  NVARCHAR (MAX) NULL,
    [ClaimValue] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);






GO
CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId]
    ON [dbo].[AspNetUserClaims]([UserId] ASC);

