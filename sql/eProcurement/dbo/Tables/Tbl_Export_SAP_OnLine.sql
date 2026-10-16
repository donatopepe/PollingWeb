/****** Object:  Table [dbo].[Tbl_Export_SAP_OnLine]    Script Date: 18/07/2022 10:15:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Export_SAP_OnLine]') AND type in (N'U'))
DROP TABLE [dbo].[Tbl_Export_SAP_OnLine]
GO
/****** Object:  Table [dbo].[Tbl_Export_SAP_OnLine]    Script Date: 18/07/2022 10:15:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tbl_Export_SAP_OnLine](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Riga] [nvarchar](max) NULL,
	[Spid] [int] NULL,
	[Codist] [smallint] NULL,
	[Elaborato] [bit] NULL,
	[Tiporecord] [nvarchar](3) NULL,
	[NumRda] [nvarchar](10) NULL,
 CONSTRAINT [PK_Tbl_Export_SAP_OnLine] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
