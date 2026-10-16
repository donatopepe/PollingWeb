/****** Object:  Table [dbo].[Log_ODA_SAP]    Script Date: 18/07/2022 10:15:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Log_ODA_SAP]') AND type in (N'U'))
DROP TABLE [dbo].[Log_ODA_SAP]
GO
/****** Object:  Table [dbo].[Log_ODA_SAP]    Script Date: 18/07/2022 10:15:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Log_ODA_SAP](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Istituto] [int] NULL,
	[Data_Put] [datetime] NULL,
	[Nrighe_input] [int] NULL,
	[Size_input] [int] NULL,
	[Data_Get] [datetime] NULL,
	[Nrighe_output] [int] NULL,
	[Size_output] [int] NULL
) ON [PRIMARY]
GO
