namespace SNR_BGC.Utilities
{
    public class BarcodeConverter
    {
        public string UPCConverter(string UPC)
        {
            string UPC_A = string.Empty;
            if (UPC.Length == 8)
            {
                string NoFirstLastCharUPC = (UPC.Substring(0, UPC.Length - 1)).Substring(1);
                char LastChar = UPC[UPC.Length - 1];

                char First = NoFirstLastCharUPC[0];
                char Second = NoFirstLastCharUPC[1];
                char Third = NoFirstLastCharUPC[2];
                char Fourth = NoFirstLastCharUPC[3];
                char Fifth = NoFirstLastCharUPC[4];
                char Sixth = NoFirstLastCharUPC[5];

               
                if (Sixth == '0')
                {
                    UPC_A = "0" + First.ToString() + Second.ToString() + "00000" + Third.ToString() + Fourth.ToString() + Fifth.ToString() + LastChar.ToString();
                }
                else if (Sixth == '1')
                {
                    UPC_A = "0" + First.ToString() + Second.ToString() + Sixth.ToString() + "0000" + Third.ToString() + Fourth.ToString() + Fifth.ToString() + LastChar.ToString();
                }
                else if (Sixth == '2')
                {
                    UPC_A = "0" + First.ToString() + Second.ToString() + Sixth.ToString() + "0000" + Third.ToString() + Fourth.ToString() + Fifth.ToString() + LastChar.ToString();
                }
                else if (Sixth == '3')
                {
                    UPC_A = "0" + First.ToString() + Second.ToString() + Third.ToString() + "00000" + Fourth.ToString() + Fifth.ToString() + LastChar.ToString();
                }
                else if (Sixth == '4')
                {
                    UPC_A = "0" + First.ToString() + Second.ToString() + Third.ToString() + Fourth.ToString() + "00000" + Fifth.ToString() + LastChar.ToString();
                }
                else if (Sixth == '5')
                {
                    UPC_A = "0" + First.ToString() + Second.ToString() + Third.ToString() + Fourth.ToString() + Fifth.ToString() + "0000" + Sixth.ToString() + LastChar.ToString();
                }
                else if (Sixth == '6')
                {
                    UPC_A = "0" + First.ToString() + Second.ToString() + Third.ToString() + Fourth.ToString() + Fifth.ToString() + "0000" + Sixth.ToString() + LastChar.ToString();
                }
                else if (Sixth == '7')
                {
                    UPC_A = "0" + First.ToString() + Second.ToString() + Third.ToString() + Fourth.ToString() + Fifth.ToString() + "0000" + Sixth.ToString() + LastChar.ToString();
                }
                else if (Sixth == '8')
                {
                    UPC_A = "0" + First.ToString() + Second.ToString() + Third.ToString() + Fourth.ToString() + Fifth.ToString() + "0000" + Sixth.ToString() + LastChar.ToString();
                }
                else if (Sixth == '9')
                {
                    UPC_A = "0" + First.ToString() + Second.ToString() + Third.ToString() + Fourth.ToString() + Fifth.ToString() + "0000" + Sixth.ToString() + LastChar.ToString();
                }



            }

            return (UPC_A);

        }

    }
}
