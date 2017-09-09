using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Procon2017MCTProtocol;
using System.ServiceModel;

namespace PuzzleSupporter.Network {
    public static class WCF{
        public static IProconPuzzleService StartWCFSender() {
            BasicHttpBinding binding = new BasicHttpBinding();
            ChannelFactory<IProconPuzzleService> factory = new ChannelFactory<IProconPuzzleService>(binding, new EndpointAddress(Parameter.ProconPuzzUri));
            return factory.CreateChannel();
        }

        public class Dummy : IProconPuzzleService {
            public void Polygon(SendablePolygon poly) { }
            public void QRCode(QRCodeData data) { }
        }
    }
}
