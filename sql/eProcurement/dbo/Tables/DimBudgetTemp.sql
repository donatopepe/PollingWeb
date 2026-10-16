/****** Object:  Table [dbo].[DimBudgetTemp]    Script Date: 18/07/2022 10:15:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DimBudgetTemp]') AND type in (N'U'))
DROP TABLE [dbo].[DimBudgetTemp]
GO
/****** Object:  Table [dbo].[DimBudgetTemp]    Script Date: 18/07/2022 10:15:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DimBudgetTemp](
	[Istituto] [smallint] NOT NULL,
	[CodCDR] [nvarchar](4) NOT NULL,
	[Materiale] [nvarchar](10) NOT NULL,
	[BudgetRilasci] [float] NOT NULL,
	[BudgetResiduo] [float] NOT NULL,
	[CodFondo] [nvarchar](50) NOT NULL,
	[Datarif] [datetime] NOT NULL,
 CONSTRAINT [PK_DimBudgetTemp] PRIMARY KEY CLUSTERED 
(
	[Istituto] ASC,
	[CodCDR] ASC,
	[Materiale] ASC,
	[BudgetRilasci] ASC,
	[BudgetResiduo] ASC,
	[CodFondo] ASC,
	[Datarif] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
