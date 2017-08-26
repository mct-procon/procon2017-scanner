using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleSupporter.Parser {
    public class PolygonParser {
        private List<Procon2017MCTProtocol.SendablePolygon> polygons;
        private List<Procon2017MCTProtocol.SendablePolygon> frames;
        private int polygons_count;

        private PolygonParser() { }

        public Procon2017MCTProtocol.QRCodeData SendData => new Procon2017MCTProtocol.QRCodeData(polygons, frames);

        public static Task<PolygonParser> Read(string Data) {
            return Task.Run(() => { 
                string QRCode = (string)Data.Clone();
                PolygonParser result = new PolygonParser();
                int itr = 0;
                for (; itr < QRCode.Length; ++itr) {
                    if (QRCode[itr] == ':')
                        break;
                }
                result.polygons_count = int.Parse(QRCode.Substring(0, itr));
                result.polygons = new List<Procon2017MCTProtocol.SendablePolygon>(result.polygons_count);

                itr++;

                int itr_start = itr;
                Procon2017MCTProtocol.SendablePolygon buffer_polygon = new Procon2017MCTProtocol.SendablePolygon();
                Procon2017MCTProtocol.SendablePoint buffer_point = new Procon2017MCTProtocol.SendablePoint();
                bool IsCheckX = true;
                for (; itr < QRCode.Length && result.polygons.Count < result.polygons_count; ++itr) {
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
                    result.polygons.Add(buffer_polygon);
                }

                result.frames = new List<Procon2017MCTProtocol.SendablePolygon>();
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
                    result.frames.Add(buffer_polygon);
                }
                return result;
            });
        }

        public Task Append(string QRCode) {
            return Task.Run(() => {
                int itr = 0, itr_start = 0;
                bool IsCheckX = true;
                Procon2017MCTProtocol.SendablePolygon buffer_polygon = new Procon2017MCTProtocol.SendablePolygon();
                Procon2017MCTProtocol.SendablePoint buffer_point = new Procon2017MCTProtocol.SendablePoint();
                if (polygons.Count < polygons_count) {
                    for (; itr < QRCode.Length && polygons.Count < polygons_count; ++itr) {
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
                        polygons.Add(buffer_polygon);
                    }
                }

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
                    frames.Add(buffer_polygon);
                }
            });
        }
    }
}
