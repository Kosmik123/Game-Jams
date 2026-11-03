namespace MicrojamEvolution.Creature
{
    /// <summary>
    /// Player data shared between the editor and the action mode: the committed creature form and the DNA.
    /// Static for now, so it lives as long as the app does.
    /// </summary>
    public static class PlayerProfile
    {
        private static Genome form;
        private static int? dna;

        public static Genome Form
        {
            get { return form ??= Genome.CreateStarting(); }
            set { form = value; }
        }

        public static int Dna
        {
            get { return dna ?? CreatureConfig.Default.StartingDna; }
            set { dna = value; }
        }
    }
}
