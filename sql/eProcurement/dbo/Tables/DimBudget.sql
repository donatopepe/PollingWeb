IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_DimBudget_IsActive' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[DimBudget] DROP CONSTRAINT [DF_DimBudget_IsActive];
END
GO
/****** Object:  Table [dbo].[DimBudget]    Script Date: 18/07/2022 10:15:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DimBudget]') AND type in (N'U'))
DROP TABLE [dbo].[DimBudget]
GO
/****** Object:  Table [dbo].[DimBudget]    Script Date: 18/07/2022 10:15:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DimBudget](
	[IDBdg] [int] IDENTITY(1,1) NOT NULL,
	[IstID] [int] NOT NULL,
	[CodFondo] [nvarchar](50) NOT NULL,
	[SapID] [int] NOT NULL,
	[Materiale] [nvarchar](10) NOT NULL,
	[CodCdR] [nvarchar](4) NOT NULL,
	[BudgetAttuale] [float] NULL,
	[BudgetResiduo] [float] NULL,
	[DataRif] [datetime] NOT NULL,
	[OrdiniAperti] [float] NULL,
	[Fatture] [float] NULL,
	[AltriUtilizzi] [float] NULL,
	[TotaleUtilizzi] [float] NULL,
	[IsActive] [int] NULL,
 CONSTRAINT [PK_DimBudget] PRIMARY KEY CLUSTERED 
(
	[IstID] ASC,
	[CodFondo] ASC,
	[SapID] ASC,
	[CodCdR] ASC,
	[DataRif] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
