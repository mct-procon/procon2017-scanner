using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Procon2017MCTProtocol;
using System.ServiceModel;

namespace PuzzleSupporter.Network {
    public static class WCF{
        public static ProconPuzzleService StartWCFSender(bool[] AvaibableSolvers) {
            return new ProconPuzzleService(AvaibableSolvers);
        }

        public class Dummy : IProconPuzzleService {
            public void Polygon(SendablePolygon poly) { }
            public void QRCode(QRCodeData data) { }
        }

        public class ProconPuzzleService : IProconPuzzleService{
            IProconPuzzleService[] services = new IProconPuzzleService[3];
            internal Exception[] exceptions = new Exception[3];
            public ProconPuzzleService(bool[] AvaibableSolvers) {
                for(int i = 0; i < 3; ++i)
                    if(i < AvaibableSolvers.Length && AvaibableSolvers[i])
                        try {
                            services[i] = new ChannelFactory<IProconPuzzleService>(new BasicHttpBinding(), new EndpointAddress(Parameter.ProconPuzzUri[i])).CreateChannel();
                        } catch(Exception ex) {
                            exceptions[i] = ex;
                            services[i] = new Dummy();
                        }
                    else
                        services[i] = new Dummy();
            }

            public void Polygon(SendablePolygon poly) {
                int err = 0;
                for (int i = 0; i < 3; ++i)
                    try {
                        services[i].Polygon(poly);
                    } catch {
                        err++;
                    }
                if (err == 3)
                    throw new Exception();
            }

            public void QRCode(QRCodeData codeData) {
                int err = 0;
                for (int i = 0; i < 3; ++i)
                    try {
                        services[i].QRCode(codeData);
                    } catch {
                        err++;
                    }
                if (err == 3)
                    throw new Exception();
            }
        }
    }
}
