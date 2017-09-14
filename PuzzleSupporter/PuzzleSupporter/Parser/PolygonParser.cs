using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleSupporter.Parser {
    public class PolygonParser {

        private PolygonParser() { }

        List<Procon2017MCTProtocol.SendablePolygon> Polygons;
        List<Procon2017MCTProtocol.SendablePolygon> Frames;

        public Procon2017MCTProtocol.QRCodeData SendData => new Procon2017MCTProtocol.QRCodeData(Polygons, Frames);

        public static async Task<PolygonParser> Read(string Data) { 
            PolygonParser result = new PolygonParser();
            await result.Append(Data);
            return result;
        }

        public Task Append(string Data) {
            string QRCode = (string)Data.Clone();
            return Task.Run(() => {
                int polygons_count;
                int itr = 0;
                for (; itr < QRCode.Length; ++itr) {
                    if (QRCode[itr] == ':')
                        break;
                }
                polygons_count = int.Parse(QRCode.Substring(0, itr));
                if (Polygons == null)
                    Polygons = new List<Procon2017MCTProtocol.SendablePolygon>(polygons_count);
                else
                    Polygons.Capacity = polygons_count + Polygons.Count;

                itr++;

                int itr_start = itr;
                Procon2017MCTProtocol.SendablePolygon buffer_polygon = new Procon2017MCTProtocol.SendablePolygon();
                Procon2017MCTProtocol.SendablePoint buffer_point = new Procon2017MCTProtocol.SendablePoint();
                bool IsCheckX = true;
                for (; itr < QRCode.Length && Polygons.Count < Polygons.Capacity; ++itr) {
                    for (; itr < QRCode.Length; ++itr) {
                        if (QRCode[itr] == ' ') {
                            buffer_polygon = new Procon2017MCTProtocol.SendablePolygon(int.Parse(QRCode.Substring(itr_start, itr - itr_start)));
                            break;
                        }
                    }
                    itr_start = itr + 1;
                    ++itr;
                    for (; itr < QRCode.Length; ++itr) {
                        if (QRCode[itr] == ' ') {
                            if (IsCheckX)
                                buffer_point.X = int.Parse(QRCode.Substring(itr_start, itr - itr_start));
                            else {
                                buffer_point.Y = int.Parse(QRCode.Substring(itr_start, itr - itr_start));
                                buffer_polygon.Points.Add(buffer_point);
                            }
                            IsCheckX = !IsCheckX;
                            itr_start = itr + 1;
                        } else if (QRCode[itr] == ':') {
                            if (IsCheckX)
                                throw new FormatException();
                            buffer_point.Y = int.Parse(QRCode.Substring(itr_start, itr - itr_start));
                            buffer_polygon.Points.Add(buffer_point);
                            itr++;
                            itr_start = itr;
                            IsCheckX = true;
                            break;
                        }
                    }
                    Polygons.Add(buffer_polygon);
                }

                if(Frames == null)
                    Frames = new List<Procon2017MCTProtocol.SendablePolygon>();
                for (; itr < QRCode.Length; ++itr) {
                    for (; itr < QRCode.Length; ++itr) {
                        if (QRCode[itr] == ' ') {
                            buffer_polygon = new Procon2017MCTProtocol.SendablePolygon(int.Parse(QRCode.Substring(itr_start, itr - itr_start)));
                            break;
                        }
                    }
                    itr_start = itr + 1;
                    ++itr;
                    for (; itr < QRCode.Length; ++itr) {
                        if (QRCode[itr] == ' ') {
                            if (IsCheckX)
                                buffer_point.X = int.Parse(QRCode.Substring(itr_start, itr - itr_start));
                            else {
                                buffer_point.Y = int.Parse(QRCode.Substring(itr_start, itr - itr_start));
                                buffer_polygon.Points.Add(buffer_point);
                            }
                            IsCheckX = !IsCheckX;
                            itr_start = itr + 1;
                        } else if (QRCode[itr] == ':') {
                            if (IsCheckX)
                                throw new FormatException();
                            buffer_point.Y = int.Parse(QRCode.Substring(itr_start, itr - itr_start));
                            buffer_polygon.Points.Add(buffer_point);
                            itr++;
                            itr_start = itr;
                            IsCheckX = true;
                            break;
                        }
                    }
                    Frames.Add(buffer_polygon);
                }
            });
        }
    }
}
