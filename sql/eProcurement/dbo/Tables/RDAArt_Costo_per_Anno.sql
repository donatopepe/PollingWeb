/****** Object:  Table [dbo].[RDAArt_Costo_per_Anno]    Script Date: 18/07/2022 10:15:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RDAArt_Costo_per_Anno]') AND type in (N'U'))
DROP TABLE [dbo].[RDAArt_Costo_per_Anno]
GO
/****** Object:  Table [dbo].[RDAArt_Costo_per_Anno]    Script Date: 18/07/2022 10:15:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RDAArt_Costo_per_Anno](
	[GUIDArt] [uniqueidentifier] NOT NULL,
	[DataInizioAnno] [datetime] NOT NULL,
	[DataFineAnno] [datetime] NULL,
	[GGAnno] [smallint] NULL,
	[CostoAnno] [float] NULL,
	[AnnoCompetenza] [smallint] NULL,
 CONSTRAINT [PK_RDAArt_Costo_per_Anno] PRIMARY KEY CLUSTERED 
(
	[GUIDArt] ASC,
	[DataInizioAnno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
