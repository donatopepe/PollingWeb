


CREATE Procedure [dbo].[Flusso_Ordini_SAP]

(
--@pi_istituto smallint,
@pi_guid	varchar(40),
--
@po_valoreuscita varchar(40) output
)



AS

declare
--gestione errori
@DBrowcount int,
@DBerror int,
@DBreturn_value int,
@PROCname varchar(50),
@tiporecord varchar(3),
 @GUID uniqueidentifier,
 @NOrdine nvarchar(1000),
 @Istituto int,
 @RigaOrdine int,
 @MesiPeriodo int,
 @MesiTOT int,
 @GiorniPeriodo int,
 @CostoPerGiorno float,
 @FlagFineAnno int,
 @TipoOrd nvarchar(2),
 @OggettoRDA nvarchar(250),
 @DataInizioCompetenzaRDA datetime,
 @DataFineCompetenzaRDA datetime,
 @Frequenza int,
 @Riga_master nvarchar(1000),
 @Riga_slave nvarchar(1000),
--DECLARE @NOrdine nvarchar(1000)
--DECLARE @TipoOrd nvarchar(2)
--DECLARE @OggettoRDA nvarchar(250)
 @TipoContabilizzazione nvarchar(1),
 @Materiale nvarchar(150),
 @GruppoMerci nvarchar(9),
 @TestoBreve nvarchar(150),
 @TestoDesc nvarchar(150),
 @CodiceIVA nvarchar(2),
 @QtaC int,
--DECLARE @i int
 @CostoTotale float,
 @Per int,
 @DataInizioCompetenzaArt datetime,
 @DataFineCompetenzaArt datetime,
 @DataPrevCons datetime,
 @Cespite nvarchar(12),
 @Fondo nvarchar(10),
 @Categ1 nvarchar(10),
 @DIC datetime,	/* DataInizioCompetenza per riga */
 @DFC datetime,	/* DataFineCompetenza per riga */
 @DFC_END datetime,
 @CostoPeriodo float,	/* Costo per riga */
 @CostoPerMese float,
 @DIC_NEXT datetime,
 @DIC_Print datetime,
 @DIC_NEXT_BK datetime,
--declare @dir nvarchar(400)
--declare @file_Name nvarchar(400)
--declare @file_Size nvarchar(400)
--declare @num_righe int
--declare @chk int
 @CdR nvarchar(5),
 @sLineR varchar(MAX),


@DBfetch_status int

--none procedura
set @PROCname = 'Flusso_Ordini_SAP'
set @tiporecord ='ODA'
--print @pi_istituto

declare @tbl_output table(
	riga nvarchar(1000)
)


SELECT 
	@GUID=t.GUID, @NOrdine=t.NOrdine, @Istituto=t.Istituto, @Frequenza=t.Frequenza, 
	@DataInizioCompetenzaRDA=t.DataInizioCompetenza,@DataFineCompetenzaRDA= t.DataFineCompetenza, 
	@TipoOrd=t.TipoOrd, @OggettoRDA=dbo.StripHTML(t.Oggetto) , 
	@Riga_master=
		t.TipoRecord + t.NOrdine + t.TipoOrdine + t.Societa + t.OrgAcquisti + 
		t.GruppoAcquisti + t.PotereDelegato + t.Fornitore + t.TestoDiTestata + t.TestoPiede 
		FROM (
			SELECT 
				GUID, Istituto, Frequenza, DataInizioCompetenza, DataFineCompetenza, TipoOrdine as TipoOrd, Oggetto,
				'T' as TipoRecord,

				(case COALESCE(TipoOrdine,'') when 'NB' then '45' when 'NC' then '47' end) + 
				(case COALESCE(cast(Istituto as nvarchar),'') when '47' then '01' when '46' then '02' end) + 
				right( REPLICATE('0', 6) + COALESCE(cast(Progressivo as nvarchar),'') , 6) as NOrdine,

				left(COALESCE(TipoOrdine,'') + 
				(case COALESCE(cast(Istituto as nvarchar),'') when '47' then '01' when '46' then '02' end) +
				REPLICATE(' ', 4) , 4) as TipoOrdine,

				left('10' + COALESCE(cast(Istituto as nvarchar),'') + REPLICATE(' ', 4) , 4) as Societa,

				left('10' + COALESCE(cast(Istituto as nvarchar),'') + REPLICATE(' ', 4) , 4) as OrgAcquisti,

				left(COALESCE(GruppoAcquisti,'') + REPLICATE(' ', 3) , 3) as GruppoAcquisti,
				left(COALESCE(PotereDelegato,'') + REPLICATE(' ', 2) , 2) as PotereDelegato,
				right(REPLICATE('0', 10) + COALESCE(cast(CFor as nvarchar),'') , 10) as Fornitore,
				left(dbo.StripHTML(COALESCE(NoteAggTestata,'')) + REPLICATE(' ', 255) , 255) as TestoDiTestata,
				left(dbo.StripHTML(COALESCE(NoteAggPiede,'')) + REPLICATE(' ', 255) , 255) as TestoPiede
				FROM eProcurement.dbo.RdA
				--WHERE (Status = 15) AND (Istituto = @Ist) AND (TipoOrdine = 'NB' OR TipoOrdine = 'NC')
				--non dovrebbe servire perche' la chiamata sara' di status=15 e Tipo NB o NC
				where 
				--Istituto=@pi_istituto and 
				GUID=@pi_guid
				) as t
				
	
	print '@pi_guid ' +@pi_guid 
	print 'NOrdine ' +@NOrdine

	--CANCELLO EVENTUALE RIGHE PRECEDENTE DI GUID NON ANCORA INVIATA E MODIFICATA DALL'UTENTE
	delete eProcurement.dbo.Tbl_Export_SAP_OnLine where substring(Riga,2,10)=@NOrdine and Tiporecord=@tiporecord


	insert into @tbl_output(riga) values (@Riga_master)	

	set @RigaOrdine = 0

	--Calcolo il codice CdR
	SELECT  @CdR = DimCentridiResp.CodiceColl
	FROM      
			eProcurement.dbo.RdA INNER JOIN
            eProcurement.dbo.DimCentridiResp ON RdA.CdRUOCode = DimCentridiResp.UoCode
	WHERE     (RdA.GUID = @GUID)


	--ciclo su RDAart
	DECLARE cur_slave CURSOR FOR
	SELECT DISTINCT 
        RDAArt.TipoContabilizzazione, SCSAP.Materiale, RDAArt.GruppoMerci, dbo.StripHTML(coalesce(RDAArt.DescrDett_Ord, @OggettoRDA)) AS TestoBreve, RDAArt.CodiceIVAConcordato, RDAArt.QtaC, RDAArt.CostoConcordatoNoIva, RDAArt.Per, RDAArt.DataInizioCompetenza, RDAArt.DataFineCompetenza, RDAArt.DataPrevCons, RDAArt.Fondo, RDAArt.Cespite, RDAArt.Categ1
	FROM	eProcurement.dbo.DimSottoCatSAP AS SCSAP INNER JOIN
            eProcurement.dbo.JoinCatSottoCat ON SCSAP.IDCatSAP = JoinCatSottoCat.SapID INNER JOIN
            eProcurement.dbo.RDAArt AS RDAArt ON eProcurement.dbo.JoinCatSottoCat.SapID = RDAArt.SapID
	WHERE  (RDAArt.RdAID = @GUID)

	OPEN cur_slave
	FETCH NEXT FROM cur_slave INTO @TipoContabilizzazione,
									@Materiale,
									@GruppoMerci,
									@TestoBreve,
									@CodiceIVA,
									@QtaC,
									@CostoTotale,
									@Per,
									@DataInizioCompetenzaArt,
									@DataFineCompetenzaArt,
									@DataPrevCons,
									@Fondo,
									@Cespite,
									@Categ1

		WHILE @@FETCH_STATUS = 0
		BEGIN 
			If (@TipoOrd='NB') begin
				set @DIC = @DataPrevCons
				set @DFC = @DataPrevCons
				set @DFC_END = @DFC
				set @CostoPeriodo = @CostoTotale
				set @DIC_NEXT = @DataPrevCons
				set @DIC_Print = @DIC
			end 
			else if (@TipoOrd='NC') and (@Frequenza > 0) begin

				if ((@DataInizioCompetenzaArt is null) or (@DataFineCompetenzaArt is null)) begin
					set @DataInizioCompetenzaArt = @DataInizioCompetenzaRDA
					set @DataFineCompetenzaArt = @DataFineCompetenzaRDA
				end
				set @MesiPeriodo = (12/@Frequenza)
				set @MesiTOT = datediff(d, @DataInizioCompetenzaArt, @DataFineCompetenzaArt)/30
				set @CostoPerMese = (@CostoTotale/@MesiTOT)
				set @DIC = @DataInizioCompetenzaArt
				set @DIC_NEXT = dateadd(m, @MesiPeriodo, @DIC)	/* data inizio competenza periodo successivo */
				set @DFC_END = dateadd(m, @MesiTOT, @DIC)	/* data fine periodo per far terminare ciclo while */
				set @FlagFineAnno = 0
			end		
			else If (@TipoOrd='NC') and (@Frequenza = 0) begin

				if ((@DataInizioCompetenzaArt is null) or (@DataFineCompetenzaArt is null)) begin
					set @DataInizioCompetenzaArt = @DataInizioCompetenzaRDA
					set @DataFineCompetenzaArt = @DataFineCompetenzaRDA
				end
				set @DIC = @DataInizioCompetenzaArt
				set @DFC = @DataInizioCompetenzaArt
				set @DFC_END = @DFC
				set @CostoPeriodo = @CostoTotale
				set @DIC_NEXT = @DataInizioCompetenzaArt
				set @DIC_Print = @DIC
			end
			

			while (@DIC_NEXT <= @DFC_END) begin

				set @sLineR = ''
				set @TestoDesc = @TestoBreve 
				--//-----------------------------------------------------//
				if (@TipoOrd='NB') begin
					set @DIC_NEXT = dateadd(m, 1, @DIC_NEXT) /*per uscire dal while dopo la prima iterazione*/
				end
				else if (@TipoOrd = 'NC') and (@Frequenza > 0) begin

					set @DFC = dateadd(d, -1, @DIC_NEXT)	/* caso standard: fine periodo */
					set @CostoPeriodo = (@CostoPerMese*@MesiPeriodo)	/* caso standard */
					
					if (@FlagFineAnno = 1) begin	/* calcolo @CostoPeriodo da inizio anno */
						set @GiorniPeriodo = datediff(d, @DIC, @DIC_NEXT)
						set @CostoPeriodo = (@CostoPerGiorno*@GiorniPeriodo) /* @CostoPerGiorno è calcolato nel ciclo precedente */
						set @FlagFineAnno = 0
						set @CostoPerGiorno = 0
					end
					/*verifico se sono a cavallo tra due anni, escludendo il caso del 31/12 come fine periodo*/
					if (datepart(yyyy, @DIC_NEXT) <> datepart(yyyy, @DIC)) and (@DFC <> cast(('31/12/' + cast(datepart(yyyy, @DIC) as nvarchar)) as datetime)) begin	
						
						set @GiorniPeriodo = datediff(d, @DIC, @DIC_NEXT)	/* intero periodo */
						set @CostoPerGiorno = (@CostoPeriodo/@GiorniPeriodo)
						set @DIC_NEXT_BK = @DIC_NEXT	/* conservo la data di fine periodo */
						set @DIC_NEXT = cast(('01/01/' + cast(datepart(yyyy, @DIC_NEXT) as nvarchar)) as datetime)
						set @GiorniPeriodo = datediff(d, @DIC, @DIC_NEXT)	/*nuovo periodo da inizio anno*/
						set @CostoPeriodo = (@CostoPerGiorno*@GiorniPeriodo)	/* calcolo @CostoPeriodo fino a fine anno */
						set @DFC = dateadd(d, -1, @DIC_NEXT)	/* fine anno */
						set @FlagFineAnno = 1
					end

					set @DIC_Print = @DIC	/* prima dell'aggiornamento, conservo il valore da stampare nel flusso */
					
					/* aggiornamento date per il periodo successivo (iterazione successiva) */
					set @DIC = @DIC_NEXT
					set @DIC_NEXT = dateadd(m, @MesiPeriodo, @DIC)
					
					if (@FlagFineAnno = 1) begin
						set @DIC_NEXT = @DIC_NEXT_BK
						set @DIC_NEXT_BK = null
					end
					
					set @TestoDesc = @TestoDesc 
										+ ' ' 
										+ cast(DATEPART (d , @DIC_Print) as nvarchar) + '/'
										+ cast(DATEPART (m , @DIC_Print) as nvarchar) + '/'
										+ cast(DATEPART (yyyy , @DIC_Print) as nvarchar)  
										+ ' - ' 
										+ cast(DATEPART (d , @DFC) as nvarchar) + '/'
										+ cast(DATEPART (m , @DFC) as nvarchar) + '/'
										+ cast(DATEPART (yyyy , @DFC) as nvarchar)  

				end /* if (@TipoOrd = 'NC') */
				else If (@TipoOrd='NC') and (@Frequenza = 0) begin
					set @DIC_NEXT = dateadd(d, 1, @DIC_NEXT) /*per uscire dal while dopo la prima iterazione*/
				end

				--//-----------------------------------------------------//
				set @sLineR = @sLineR + left('10' + COALESCE(cast(@Istituto as nvarchar),'') + REPLICATE(' ', 4) , 4)
				set @sLineR = @sLineR + left('1000' + REPLICATE(' ', 4) , 4)
				set @sLineR = @sLineR + left(COALESCE(@TipoContabilizzazione,'') + REPLICATE(' ', 1) , 1)
				set @sLineR = @sLineR + 'S' + right(COALESCE(@Materiale,'') , 7) + '-000'
				set @sLineR = @sLineR + left((case when (@GruppoMerci is not null) or (@TipoContabilizzazione = 'A') then COALESCE(@TestoDesc,'') else '' end)  + REPLICATE(' ', 50) , 50)
				set @sLineR = @sLineR + left(COALESCE(@CodiceIVA,'') + REPLICATE(' ', 2) , 2)
				set @sLineR = @sLineR + right(REPLICATE('0', 11) + COALESCE(cast(@QtaC as nvarchar),'') , 11)
				set @sLineR = @sLineR + left((case when @TipoContabilizzazione = 'Y' then 'VAL' when @TipoContabilizzazione = 'A' then 'PZ' else '' end) + REPLICATE(' ', 3) , 3)
				set @sLineR = @sLineR + right(REPLICATE('0', 11) + 
										(case when CHARINDEX('.', COALESCE(cast(convert(decimal(10,2), @CostoPeriodo) as nvarchar),'')) > 0 then
											SUBSTRING ( COALESCE(cast(convert(decimal(10,2), @CostoPeriodo) as nvarchar),''), 0 , CHARINDEX('.', COALESCE(cast(convert(decimal(10,2), @CostoPeriodo) as nvarchar),'')) )
										else COALESCE(cast(convert(decimal(10,2), @CostoPeriodo) as nvarchar),'') end) , 11)
				set @sLineR = @sLineR + left(
										(case when CHARINDEX('.', COALESCE(cast(convert(decimal(10,2), @CostoPeriodo) as nvarchar),'')) > 0 then
											SUBSTRING ( COALESCE(cast(convert(decimal(10,2), @CostoPeriodo) as nvarchar),''), CHARINDEX('.', COALESCE(cast(convert(decimal(10,2), @CostoPeriodo) as nvarchar),''))+1 , len(COALESCE(cast(convert(decimal(10,2), @CostoPeriodo) as nvarchar),'')) )
										else '00' end) 
										+ REPLICATE('0', 2) , 2)
				
				set @sLineR = @sLineR + right(REPLICATE('0', 5) + COALESCE(cast(@Per as nvarchar),''), 5)

				set @sLineR = @sLineR + right('0000' + COALESCE(cast(DATEPART (yyyy , @DIC_Print) as nvarchar),'') , 4) + 
										right('00' + COALESCE(cast(DATEPART (m , @DIC_Print) as nvarchar),'') , 2) + 
										right('00' + COALESCE(cast(DATEPART (d , @DIC_Print) as nvarchar),'') , 2)

				set @sLineR = @sLineR + right('0000' + COALESCE(cast(DATEPART (yyyy , @DFC) as nvarchar),'') , 4) + 
										right('00' + COALESCE(cast(DATEPART (m , @DFC) as nvarchar),'') , 2) + 
										right('00' + COALESCE(cast(DATEPART (d , @DFC) as nvarchar),'') , 2)

				set @sLineR = @sLineR + left(COALESCE(@GruppoMerci,'') + REPLICATE(' ', 9) , 9)
				set @sLineR = @sLineR + left('' + REPLICATE(' ', 10) , 10)
				set @sLineR = @sLineR + left('' + REPLICATE(' ', 14) , 14)
				set @sLineR = @sLineR + left(COALESCE(@CdR,'') + REPLICATE(' ', 10) , 10)
				set @sLineR = @sLineR + left(COALESCE(@Fondo,'') + REPLICATE(' ', 10) , 10)
				set @sLineR = @sLineR + left(COALESCE(@Cespite,'') + REPLICATE(' ', 12) , 12)
				set @sLineR = @sLineR + left('' + REPLICATE(' ', 4) , 4)
				set @sLineR = @sLineR + left(COALESCE(@Categ1,'') + REPLICATE(' ', 10) , 10)
				set @sLineR = @sLineR + left('' + REPLICATE(' ', 255) , 255)
				--//-----------------------------------------------------//
				set @RigaOrdine = @RigaOrdine + 10

				set @Riga_slave = 'R' + @NOrdine + right('000' + cast(@RigaOrdine as nvarchar) , 3) + @sLineR

				insert into @tbl_output(riga) values (@Riga_slave)
				
			end  --while

			FETCH NEXT FROM cur_slave INTO @TipoContabilizzazione,
											@Materiale,
											@GruppoMerci,
											@TestoBreve,
											@CodiceIVA,
											@QtaC,
											@CostoTotale,
											@Per,
											@DataInizioCompetenzaArt,
											@DataFineCompetenzaArt,
											@DataPrevCons,
											@Fondo,
											@Cespite,
											@Categ1
		END
		CLOSE cur_slave
		DEALLOCATE cur_slave


fine:

INSERT INTO eProcurement.dbo.Tbl_Export_SAP_OnLine(Riga, Spid,Codist,Tiporecord)
		SELECT riga, @@SPID,@Istituto,@tiporecord
		FROM @tbl_output

--restituisco il valore
set @po_valoreuscita=''


--success
return 0



GRANT EXEC ON Flusso_Ordini_SAP TO PUBLIC

