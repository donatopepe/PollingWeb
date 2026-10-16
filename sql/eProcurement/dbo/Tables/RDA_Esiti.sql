/****** Object:  Table [dbo].[RDA_Esiti]    Script Date: 18/07/2022 10:15:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RDA_Esiti]') AND type in (N'U'))
DROP TABLE [dbo].[RDA_Esiti]
GO
/****** Object:  Table [dbo].[RDA_Esiti]    Script Date: 18/07/2022 10:15:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RDA_Esiti](
	[Mandante] [nchar](3) NULL,
	[Societa] [nchar](4) NULL,
	[Num_Ordine] [nchar](10) NULL,
	[Posizione] [nchar](3) NULL,
	[Cod_Errore] [nvarchar](5) NULL,
	[Desc_Errore] [nvarchar](55) NULL,
	[Data_FileOutput] [datetime] NULL
) ON [PRIMARY]
GO
