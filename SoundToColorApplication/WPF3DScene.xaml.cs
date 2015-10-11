using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SoundToColorApplication
{
    /// <summary>
    /// Interaction logic for WPF3DScene.xaml
    /// </summary>
    public partial class WPF3DScene : UserControl
    {
        private double _azimuthAngle = 1 * Math.PI / 4;
        private double _polarAngle = 1 * Math.PI / 6;
        private double _radialDistance = 4;

        public WPF3DScene()
        {
            InitializeComponent();
            
            UpdateCamera();
        }

        public void UpdateCamera()
        {

            var xCoord = Math.Sin(_azimuthAngle) * Math.Cos(_polarAngle) * _radialDistance;
            var yCoord = -Math.Cos(_azimuthAngle) * Math.Cos(_polarAngle) * _radialDistance;
            var zCoord = Math.Sin(_polarAngle) * _radialDistance;

            var xCoord2 = -Math.Sin(_azimuthAngle) * Math.Sin(_polarAngle);
            var yCoord2 = Math.Cos(_azimuthAngle) * Math.Sin(_polarAngle);
            var zCoord2 = Math.Cos(_polarAngle);

            Camera.Position = new Point3D(xCoord, yCoord, zCoord);
            Camera.LookDirection = -new Vector3D(xCoord, yCoord, zCoord);
            Camera.UpDirection = new Vector3D(xCoord2, yCoord2, zCoord2);
        }

        public void AddModel(GeometryModel3D model)
        {
            Models.Children.Add(model);
        }

        private bool _dragging = false;
        private Point _latestMousePoint; 
        private void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragging = true;
            _latestMousePoint = e.GetPosition(this);
        }

        private void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragging = false;
        }

        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging)
                return;

            var newPoint = e.GetPosition(this);

            var displacement = newPoint - _latestMousePoint;
            _latestMousePoint = newPoint;

            _polarAngle = (_polarAngle + displacement.Y/50) % (2 * Math.PI);
            _azimuthAngle = (_azimuthAngle - displacement.X/50) % (2 * Math.PI);

            UpdateCamera();
        }
    }
}
