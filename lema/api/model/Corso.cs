using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.model
{
    [Table("corsi")]
    public class Corso
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("codice_corso")]
        [MaxLength(50)]
        public string? CodiceCorso { get; set; }

        [Column("denominazione_attuale_corso")]
        public string? DenominazioneAttualeCorso { get; set; }

        [Column("descrizione_abbreviata")]
        public string? DescrizioneAbbreviata { get; set; }

        [Column("descrizione_estesa")]
        public string? DescrizioneEstesa { get; set; }

        [Column("data_istituzione")]
        public DateOnly? DataIstituzione { get; set; }

        [Column("motivo_istituzione")]
        public string? MotivoIstituzione { get; set; }

        [Column("sospeso_soppresso")]
        public bool? SospesoSoppresso { get; set; }

        [Column("data_soppressione")]
        public DateOnly? DataSoppressione { get; set; }

        [Column("motivo_soppressione")]
        public string? MotivoSoppressione { get; set; }

        [Column("area_formazione")]
        public string? AreaFormazione { get; set; }

        [Column("settore_formazione")]
        public string? SettoreFormazione { get; set; }

        [Column("tipo_corso")]
        public string? TipoCorso { get; set; }

        [Column("denominazione_titolo")]
        public string? DenominazioneTitolo { get; set; }

        [Column("codice___titolo")]
        [MaxLength(50)]
        public string? CodiceTitolo { get; set; }

        [Column("brevetto_associato_corso")]
        public string? BrevettoAssociatoCorso { get; set; }

        [Column("ente_programmatore")]
        public string? EnteProgrammatore { get; set; }

        [Column("ente_erogatore")]
        public string? EnteErogatore { get; set; }

        [Column("durata_gg_add")]
        public int? DurataGgAdd { get; set; }

        [Column("frequentatori")]
        public string? Frequentatori { get; set; }

        [Column("job_description")]
        public string? JobDescription { get; set; }

        [Column("posizione_prevista")]
        public string? PosizionePrevista { get; set; }

        [Column("modalita_selezione")]
        public string? ModalitaSelezione { get; set; }

        [Column("presente_in_n8")]
        public bool? PresenteInN8 { get; set; }

        [Column("parole_chiave")]
        public string? ParoleChiave { get; set; }

        [Column("corso_da_bonificare")]
        public bool? CorsoDaBonificare { get; set; }

        [Column("corso_da_lasciare")]
        public bool? CorsoDaLasciare { get; set; }

        [Column("corso_da_aggiungere")]
        public bool? CorsoDaAggiungere { get; set; }

        [Column("codici___associati")]
        public string? CodiciAssociati { get; set; }
    }
}