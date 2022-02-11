namespace EmotionTrainingV2
{

    internal sealed class Program
    {

        #region Fields

        private const int Size = 227;

        #endregion

        #region Methods

        private static int Main(string[] args)
        {
            var name = nameof(EmotionTrainingV2);
            var description = "The program for training Corrective re-annotation of FER - CK+ - KDEF dataset";
            //var trainer = new EmotionTrainer(Size, name, description);
            var trainer = new EmotionGrayscaleTrainer(Size, name, description);
            return trainer.Start(args);
        }
        
        #endregion

    }

}