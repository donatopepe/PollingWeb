IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_JoinCatSottoCat_DimSottoCatSAP' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[JoinCatSottoCat] DROP CONSTRAINT [FK_JoinCatSottoCat_DimSottoCatSAP];
END
GO
/****** Object:  Table [dbo].[JoinCatSottoCat]    Script Date: 18/07/2022 10:15:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JoinCatSottoCat]') AND type in (N'U'))
DROP TABLE [dbo].[JoinCatSottoCat]
GO
/****** Object:  Table [dbo].[JoinCatSottoCat]    Script Date: 18/07/2022 10:15:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JoinCatSottoCat](
	[CatID] [int] NOT NULL,
	[SCID] [int] NOT NULL,
	[SapID] [int] NOT NULL,
	[CodPrg] [nvarchar](50) NOT NULL,
	[ImportoMaxOper] [float] NULL,
	[OrdineSAP] [int] NULL,
	[ImportoMaxAut] [float] NULL,
	[IsStrategico] [int] NULL,
	[IsActive] [int] NULL,
 CONSTRAINT [PK_JoinCatSottoCat_1] PRIMARY KEY CLUSTERED 
(
	[CatID] ASC,
	[SCID] ASC,
	[SapID] ASC,
	[CodPrg] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
