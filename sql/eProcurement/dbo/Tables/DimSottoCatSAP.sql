/****** Object:  Table [dbo].[DimSottoCatSAP]    Script Date: 18/07/2022 10:15:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DimSottoCatSAP]') AND type in (N'U'))
DROP TABLE [dbo].[DimSottoCatSAP]
GO
/****** Object:  Table [dbo].[DimSottoCatSAP]    Script Date: 18/07/2022 10:15:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DimSottoCatSAP](
	[IDCatSAP] [int] NOT NULL,
	[Materiale] [nvarchar](50) NULL,
	[TestoBreve] [nvarchar](150) NULL,
 CONSTRAINT [PK_DimSottoCatSAP_1] PRIMARY KEY CLUSTERED 
(
	[IDCatSAP] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
