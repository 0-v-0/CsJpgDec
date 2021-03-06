﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using LibPixz.Markers;

namespace LibPixz
{
	public partial class Pixz
    {
        public enum Markers
        {
            LiteralFF = 0x00,
            Soi = 0xd8,
            App0 = 0xe0,
            App14 = 0xee,
            Dqt = 0xdb,
            Sof0 = 0xc0, Sof2 = 0xc2,
            Dht = 0xc4,
            Rs0 = 0xd0, Rs1 = 0xd1, Rs2 = 0xd2, Rs3 = 0xd3,
            Rs4 = 0xd4, Rs5 = 0xd5, Rs6 = 0xd6, Rs7 = 0xd7,
            Sos = 0xda,
            Eoi = 0xd9,
            Dri = 0xdd
        }

        public static List<Bitmap> Decode(string path)
        {
			//Logger.WriteLine("Log for image: " + path);
			return Decode(ReadFileToMemory(path));
        }

        public static List<Bitmap> Decode(MemoryStream stream)
        {
            var reader = new BinaryReader(stream);
            var images = new List<Bitmap>();

            stream.Seek(0, SeekOrigin.Begin);

            bool eof = false;

            for (int image = 1; ; image++)
            {
                try
                {
                    var imgInfo = new ImgInfo();

                    while (true)
                    {
                        while (reader.ReadByte() != 0xff) ;
                        int markerId = reader.ReadByte();

                        switch ((Markers)markerId)
                        {
                            case Markers.App0:
                                App0.Read(reader, imgInfo);
                                break;
                            case Markers.App14:
                                App14.Read(reader, imgInfo);
                                break;
                            case Markers.Dqt:
                                Dqt.Read(reader, imgInfo);
                                break;
                            case Markers.Sof0:
                                Sof0.Read(reader, imgInfo);
                                break;
                            case Markers.Sof2:
                                Sof2.Read(reader, imgInfo);
                                break;
                            case Markers.Dht:
                                Dht.Read(reader, imgInfo);
                                break;
                            case Markers.Sos:
                                images.Add(Sos.Read(reader, imgInfo));
                                break;
                            case Markers.Soi:
                                imgInfo = new ImgInfo();
                                //Logger.Write("Start of Image " + image);
                                //Logger.WriteLine(" at: " + (reader.BaseStream.Position - 2).ToString("X"));
                                imgInfo.startOfImageFound = true;
                                break;
                            case Markers.Dri:
                                Dri.Read(reader, imgInfo);
                                break;
                            case Markers.Eoi:
                                //Logger.Write("End of Image " + image);
                                //Logger.WriteLine(" at: " + (reader.BaseStream.Position - 2).ToString("X"));
                                eof = true;
                                break;
                            // Unknown markers, or markers used outside of their specified area
                            default:
                                Default.Read(reader, imgInfo, (Markers)markerId);
                                break;
                        }

                        if (eof)
                        {
                            eof = false;
                            break;
                        }
                    }
                }
                catch (EndOfStreamException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    //Logger.WriteLine(ex.Message);
                }
            }

            reader.Close();
            //Logger.Flush();

            return images;
        }

        protected static MemoryStream ReadFileToMemory(string path)
        {
            var stream = new MemoryStream();
            FileStream archivo = File.OpenRead(path);

            stream.SetLength(archivo.Length);
            archivo.Read(stream.GetBuffer(), 0, (int)archivo.Length);

            archivo.Close();

            return stream;
        }
    }
}
