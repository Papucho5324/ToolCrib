namespace Practicas_Demo1.Models
{
    public class FolioApproveModel
    {

        public string NoFolio { get; set; }
        public string Name { get; set; }
        public string Status { get; set; } // "Pendiente", "Aprobado", etc.
        public string Tipo { get; set; }
        public DateTime Udt { get; set; }

    }

    public class UserAccount
    {
        public string Account { get; set; }
        public string Name { get; set; }
        public string FISGroup { get; set; }
    }
}
