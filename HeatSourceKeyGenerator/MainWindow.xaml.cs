using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HeatSourceKeyGenerator
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var publicKey =
                "<RSAKeyValue><Modulus>twbSt6je14wVIDcL3jHnL73s1sX6+aOrq4ox+dW+ZRKgipkA5qTDrcIByBwG9mu/GOBpxhc/D2t0ppHOPe+Iax4Xq3gRtOFmsK8g9cgtOVkbSggZUsL+gWwVrnw0u2g0tA3M6/rSeJfWwfb3SNQMcLUKgB5lyxvV1gbjR3caFDE=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

            var privateKey =
                "<RSAKeyValue><Modulus>twbSt6je14wVIDcL3jHnL73s1sX6+aOrq4ox+dW+ZRKgipkA5qTDrcIByBwG9mu/GOBpxhc/D2t0ppHOPe+Iax4Xq3gRtOFmsK8g9cgtOVkbSggZUsL+gWwVrnw0u2g0tA3M6/rSeJfWwfb3SNQMcLUKgB5lyxvV1gbjR3caFDE=</Modulus><Exponent>AQAB</Exponent><P>6a4CNx0DKAy8F2DpgD/+kCXqqkziKtJyPwegbc4HGfnoLluCIoti3XHON63Zy03XFWYmrc1nnObabQyte+P/1w==</P><Q>yII71yF8gWSc/vYHlE7yf+ZdEBTowiEHBxj+PbTeZxvaFvLdkkbu5yRlqZ055QHjyXonqks3PedzLduuq2MrNw==</Q><DP>kSq5U5WhZVrr6jYZOMQIl3IGov8aLXII6SEKx8wK21dFpq7QXDdtFpZhfnNw7nyP1kpcUvJcoLtFDCOpDmLohw==</DP><DQ>OvvMvLF2Hd5KLNqVSjSbM7LBp6xu+ESc+6KHkAF3918DHgMd2R0SxF45pdbBpDOVHbzYlEkCHCzM/i2EyZgnqw==</DQ><InverseQ>MEonA3yfe2YOZAN5poQ/aUuDypbw2hDavbBS0z6U+fmQngv8taTYBWaHJJoy0ABnAMsDUDgnn7CoJZRxF+HL3g==</InverseQ><D>VcxLB3GQVTcDyEBFvfliyMyds+jHSDNjjFySJO3//N2IiLBE7dobKN8jeq04zlegE4dPzl6jXxUeb9818W7SYakFqhLLwczjpBapzR5uerBU6t2HxZOfREiW4r0QQJfBt9InEJbYB1tMjc63FYoVMqCc0wyErNQnNdp4m8qlskk=</D></RSAKeyValue>";

            try
            {
                // Create a UnicodeEncoder to convert between byte array and string.
                ASCIIEncoding ByteConverter = new ASCIIEncoding();

                string dataString = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd");

                // Create byte arrays to hold original, encrypted, and decrypted data.
                byte[] originalData = ByteConverter.GetBytes(dataString);
                byte[] signedData;

                // Create a new instance of the RSACryptoServiceProvider class 
                // and automatically create a new key-pair.
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

                RSAalg.FromXmlString(privateKey);
                // Export the key information to an RSAParameters object.
                // You must pass true to export the private key for signing.
                // However, you do not need to export the private key
                // for verification.
                RSAParameters Key = RSAalg.ExportParameters(true);

                // Hash and sign the data.
                signedData = HashAndSignBytes(originalData, Key);

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.FileName = "license.txt";
                saveFileDialog1.Filter = "All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == true)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(saveFileDialog1.FileName, false))
                    {
                        file.WriteLine(Convert.ToBase64String(originalData));
                        file.WriteLine(Convert.ToBase64String(signedData));
                    }
                }

                // Verify the data and display the result to the 
                // console.
                RSAalg.FromXmlString(publicKey);
                RSAParameters Key1 = RSAalg.ExportParameters(false);

                if (VerifySignedHash(originalData, signedData, Key1))
                {
                    Console.WriteLine("The data was verified.");
                }
                else
                {
                    Console.WriteLine("The data does not match the signature.");
                }

            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("The data was not signed or verified");
            }
        }

        public static byte[] HashAndSignBytes(byte[] DataToSign, RSAParameters Key)
        {
            try
            {
                // Create a new instance of RSACryptoServiceProvider using the 
                // key from RSAParameters.  
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

                RSAalg.ImportParameters(Key);

                // Hash and sign the data. Pass a new instance of SHA1CryptoServiceProvider
                // to specify the use of SHA1 for hashing.
                return RSAalg.SignData(DataToSign, new SHA1CryptoServiceProvider());
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }

        public static bool VerifySignedHash(byte[] DataToVerify, byte[] SignedData, RSAParameters Key)
        {
            try
            {
                // Create a new instance of RSACryptoServiceProvider using the 
                // key from RSAParameters.
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

                RSAalg.ImportParameters(Key);

                // Verify the data using the signature.  Pass a new instance of SHA1CryptoServiceProvider
                // to specify the use of SHA1 for hashing.
                return RSAalg.VerifyData(DataToVerify, new SHA1CryptoServiceProvider(), SignedData);

            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return false;
            }
        }
    }
}
