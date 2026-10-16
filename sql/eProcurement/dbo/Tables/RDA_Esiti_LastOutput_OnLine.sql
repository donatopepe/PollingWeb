/****** Object:  Table [dbo].[RDA_Esiti_LastOutput_OnLine]    Script Date: 18/07/2022 10:15:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RDA_Esiti_LastOutput_OnLine]') AND type in (N'U'))
DROP TABLE [dbo].[RDA_Esiti_LastOutput_OnLine]
GO
/****** Object:  Table [dbo].[RDA_Esiti_LastOutput_OnLine]    Script Date: 18/07/2022 10:15:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RDA_Esiti_LastOutput_OnLine](
	[Num_Ordine] [nchar](10) NOT NULL,
	[Mandante] [nchar](3) NULL,
	[Societa] [nchar](4) NULL,
	[Posizione] [nchar](3) NULL,
	[Cod_Errore] [nvarchar](5) NULL,
	[Desc_Errore] [nvarchar](55) NULL,
	[Data_FileOutput] [datetime] NULL,
 CONSTRAINT [PK_RDA_Esiti_LastOutput_OnLine] PRIMARY KEY CLUSTERED 
(
	[Num_Ordine] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
