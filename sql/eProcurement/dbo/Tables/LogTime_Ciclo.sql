IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_LogTime_Ciclo_Data' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[LogTime_Ciclo] DROP CONSTRAINT [DF_LogTime_Ciclo_Data];
END
GO
/****** Object:  Table [dbo].[LogTime_Ciclo]    Script Date: 18/07/2022 10:15:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LogTime_Ciclo]') AND type in (N'U'))
DROP TABLE [dbo].[LogTime_Ciclo]
GO
/****** Object:  Table [dbo].[LogTime_Ciclo]    Script Date: 18/07/2022 10:15:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LogTime_Ciclo](
	[IDTC] [int] IDENTITY(1,1) NOT NULL,
	[Data] [datetime] NULL,
	[Rch_Progr] [numeric](18, 0) NULL,
	[Rch_ID] [uniqueidentifier] NULL,
	[PageID] [int] NULL,
	[Matricola] [nvarchar](7) NULL,
	[StatoID] [int] NULL,
	[StatoIntID] [int] NULL,
	[UOCodeNext] [nvarchar](6) NULL,
	[RuoloNext] [int] NULL,
	[IstIDNext] [int] NULL,
 CONSTRAINT [PK_LogTime_Ciclo] PRIMARY KEY CLUSTERED 
(
	[IDTC] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
