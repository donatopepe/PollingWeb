CREATE TABLE [dbo].[TargetSC00G100] (
    [Istituto]        SMALLINT       NOT NULL,
    [Cfor]            INT            NOT NULL,
    [Ragione_Sociale] NVARCHAR (200) NULL,
    [Indirizzo]       NVARCHAR (50)  NULL,
    [Prov]            NVARCHAR (3)   NULL,
    [Cap]             NVARCHAR (50)  NULL,
    [Citta]           NVARCHAR (50)  NULL,
    [Codfis]          NVARCHAR (16)  NULL,
    [Piva]            NVARCHAR (11)  NULL,
    CONSTRAINT [PK_Target_1] PRIMARY KEY CLUSTERED ([Istituto] ASC, [Cfor] ASC)
);

