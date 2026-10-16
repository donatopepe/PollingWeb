/****** Object:  View [dbo].[ViewEstrazioneRdARdE]    Script Date: 18/07/2022 10:15:50 ******/
IF OBJECT_ID(N'[dbo].[ViewEstrazioneRdARdE]') IS NOT NULL
    DROP VIEW [dbo].[ViewEstrazioneRdARdE];
GO
/****** Object:  View [dbo].[ViewEstrazioneRdARdE]    Script Date: 18/07/2022 10:15:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[ViewEstrazioneRdARdE]
AS
SELECT        GUID, Progressivo, PageID, Istituto, Oggetto, TacID, TacIDBuyer, CdRUOCode, CdRIstID, CatID, Matricola, Status, UOCode, IstID, UOCode1, IstID1, Destinazione, IstIDDestinazione, CodMta, NOrdSAP, NOrdSAPInt, Motivazione, 
                         Tipomod, MotivazioneAnnulla, DatiMancanti, Multipla, Manuale, CostoTotale, CostoTotConcordato, PDAut, ImpOrd, ImpInt, DataCreazione, DataModifica, FlagDone, NoteDelibera, IsDelega, Verificato, Canale, IDRichiesta, 
                         ParteCorrelata, TipoOrdine, ForID, CFor, ForSceltoID, ForNuovo, GruppoAcquisti, PotereDelegato, Frequenza, FqID, DataInizioCompetenza, DataFineCompetenza, NoteAggTestata, NoteAggPiede, IndiConsNoSTD, GUIDCtr, 
                         CodPrg, DispBudget, IsDerogata, TipoRdAID, TpsRdAID, CompetenzeIAnno, CompetenzeIIAnno, CompetenzeIIIAnno, IsExArt81, CentroDiCosto
FROM            dbo.RdA
GO
ALTER TABLE [dbo].[DimBudget] ADD  CONSTRAINT [DF_DimBudget_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[DimCentridiResp] ADD  CONSTRAINT [DF_DimCentridiResp_RichiestaFirmaCM]  DEFAULT ((0)) FOR [RichiestaFirmaCM]
GO
ALTER TABLE [dbo].[DimCentridiResp] ADD  CONSTRAINT [DF_DimCentridiResp_FirmaCdRAutomatica_1]  DEFAULT ((0)) FOR [FirmaCdRAutomatica]
GO
ALTER TABLE [dbo].[DimFornitori] ADD  CONSTRAINT [DF_DimFornitori_IscrittoAF]  DEFAULT ((0)) FOR [IscrittoAF]
GO
ALTER TABLE [dbo].[DimFornitori] ADD  CONSTRAINT [DF_DimFornitori_InAttesa]  DEFAULT ((0)) FOR [InAttesa]
GO
ALTER TABLE [dbo].[DimFornitori] ADD  CONSTRAINT [DF_DimFornitori_Istituzionale]  DEFAULT ((0)) FOR [Istituzionale]
GO
ALTER TABLE [dbo].[DimFornitori] ADD  CONSTRAINT [DF_Fornitori_Strategico]  DEFAULT ((0)) FOR [Strategico]
GO
ALTER TABLE [dbo].[DimFornitori] ADD  CONSTRAINT [DF_DimFornitori_ParteCorrelata_1]  DEFAULT ((0)) FOR [ParteCorrelata]
GO
ALTER TABLE [dbo].[DimFornitori] ADD  CONSTRAINT [DF_DimFornitori_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[LogTime_Ciclo] ADD  CONSTRAINT [DF_LogTime_Ciclo_Data]  DEFAULT (getdate()) FOR [Data]
GO
ALTER TABLE [dbo].[RdA] ADD  CONSTRAINT [DF_RdA_Multipla]  DEFAULT ((0)) FOR [Multipla]
GO
ALTER TABLE [dbo].[RdA] ADD  CONSTRAINT [DF_RdA_FlagDone]  DEFAULT ((0)) FOR [FlagDone]
GO
ALTER TABLE [dbo].[RdA] ADD  CONSTRAINT [DF_RdA_IsDelega]  DEFAULT ((0)) FOR [IsDelega]
GO
ALTER TABLE [dbo].[RdA] ADD  CONSTRAINT [DF_RdA_ParteCorrelata]  DEFAULT ((0)) FOR [ParteCorrelata]
GO
ALTER TABLE [dbo].[RdA] ADD  CONSTRAINT [DF_RdA_Frequenza_1]  DEFAULT ((0)) FOR [Frequenza]
GO
ALTER TABLE [dbo].[RDAArt] ADD  CONSTRAINT [DF_RDAArt_CostoTotale]  DEFAULT ((0)) FOR [CostoTotale]
GO
ALTER TABLE [dbo].[RDAArt] ADD  CONSTRAINT [DF_RDAArt_CostoTotale1]  DEFAULT ((0)) FOR [CostoUnitarioConsumo]
GO
ALTER TABLE [dbo].[RDAArt] ADD  CONSTRAINT [DF_RDAArt_CostoConcordatoNoIva]  DEFAULT ((0)) FOR [CostoConcordatoNoIva]
GO
ALTER TABLE [dbo].[RDAArt] ADD  CONSTRAINT [DF_RDAArt_CostoConcordato]  DEFAULT ((0)) FOR [CostoConcordato]
GO
ALTER TABLE [dbo].[RDAArt] ADD  CONSTRAINT [DF_RDAArt_TipoContabilizzazione_1]  DEFAULT ('Y') FOR [TipoContabilizzazione]
GO
ALTER TABLE [dbo].[RDAArt] ADD  CONSTRAINT [DF_RDAArt_Per_1]  DEFAULT ((1)) FOR [Per]
GO
ALTER TABLE [dbo].[RdE] ADD  CONSTRAINT [DF_RdE_Consulenza_1]  DEFAULT ((0)) FOR [Consulenza]
GO
ALTER TABLE [dbo].[JoinCatSottoCat]  WITH CHECK ADD  CONSTRAINT [FK_JoinCatSottoCat_DimSottoCatSAP] FOREIGN KEY([SapID])
REFERENCES [dbo].[DimSottoCatSAP] ([IDCatSAP])
GO
ALTER TABLE [dbo].[JoinCatSottoCat] CHECK CONSTRAINT [FK_JoinCatSottoCat_DimSottoCatSAP]
GO
ALTER TABLE [dbo].[RDAArt]  WITH CHECK ADD  CONSTRAINT [FK_RDAArt_DimFornitori] FOREIGN KEY([ForID])
REFERENCES [dbo].[DimFornitori] ([IDFor])
GO
ALTER TABLE [dbo].[RDAArt] CHECK CONSTRAINT [FK_RDAArt_DimFornitori]
GO
ALTER TABLE [dbo].[RDAArt]  WITH CHECK ADD  CONSTRAINT [FK_RDAArt_DimFornitori1] FOREIGN KEY([ForSceltoID])
REFERENCES [dbo].[DimFornitori] ([IDFor])
GO
ALTER TABLE [dbo].[RDAArt] CHECK CONSTRAINT [FK_RDAArt_DimFornitori1]
GO
ALTER TABLE [dbo].[RDAArt]  WITH CHECK ADD  CONSTRAINT [FK_RDAArt_DimFornitori2] FOREIGN KEY([ForAlternativoID1])
REFERENCES [dbo].[DimFornitori] ([IDFor])
GO
ALTER TABLE [dbo].[RDAArt] CHECK CONSTRAINT [FK_RDAArt_DimFornitori2]
GO
ALTER TABLE [dbo].[RdE]  WITH CHECK ADD  CONSTRAINT [FK_RdE_RdA] FOREIGN KEY([RdAID])
REFERENCES [dbo].[RdA] ([GUID])
GO
ALTER TABLE [dbo].[RdE] CHECK CONSTRAINT [FK_RdE_RdA]
GO
