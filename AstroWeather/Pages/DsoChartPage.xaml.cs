using Microcharts;
using SkiaSharp;
using Microsoft.Maui.Controls;
using System.Xml.Linq;
using SkiaSharp.Views.Maui;
using System.Diagnostics;



namespace AstroWeather.Pages
{
    public partial class DsoChartPage : ContentPage
    {
        DsoObject _selectedDSO;
        public DsoChartPage(DsoObject selectedDso)
        {
            InitializeComponent();
            _selectedDSO = selectedDso;

           
            DsoNameLabel.Text = selectedDso.Name;
            DsoDetailsLabel.Text = $"Magnitude: {selectedDso.Mag}, Max Altitude: {Math.Round(selectedDso.MaxAlt)}°";
            DsosizeLabel.Text = "Size: " + selectedDso.Size.ToString() + "'";
            DsotypeLabel.Text = "Type: " + selectedDso.Type;
            DsoDescriptionLabel.Text = selectedDso.Description;
            DsoconstellationLabel.Text = "Constellation: " + selectedDso.Constellation;

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            canvasView.InvalidateSurface(); 
        }
        private void OnCanvasPaint(object sender, SKPaintSurfaceEventArgs e)
        {
            

            var canvas = e.Surface.Canvas;
            var info = e.Info;
            canvas.Clear(SKColors.Black);

            var paint = new SKPaint
            {
                Color = SKColors.Red,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            float centerX = info.Width / 2;
            float centerY = info.Height / 2;
            float scale = Math.Min(info.Width, info.Height) / 2.2f;
 
            DrawAzimuthalGrid(canvas, centerX, centerY, scale);

            DrawDsoTrajectory(canvas, centerX, centerY, scale);
        }
        private void DrawAzimuthalGrid(SKCanvas canvas, float cx, float cy, float scale)
        {
            var gridPaint = new SKPaint
            {
                Color = SKColors.Gray,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };
            float radius = (70 / 90f) * scale;
            canvas.DrawCircle(cx, cy, radius, gridPaint);
            
            for (int alt = 30; alt <= 90; alt += 30)
            {
                radius = (alt / 90f) * scale;
                canvas.DrawCircle(cx, cy, radius, gridPaint);
            }

            for (int az = 0; az < 360; az += 45)
            {
                float angleRad = MathF.PI * az / 180f;
                float x = cx + MathF.Cos(angleRad) * scale;
                float y = cy + MathF.Sin(angleRad) * scale;  

                canvas.DrawLine(cx, cy, x, y, gridPaint);
            }

            
            var textPaint = new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = 20,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };

            canvas.DrawText("N", cx, cy - scale - 5, textPaint);
            canvas.DrawText("S", cx, cy + scale + 20, textPaint); 
            canvas.DrawText("E", cx + scale + 20, cy + 5, textPaint); 
            canvas.DrawText("W", cx - scale - 20, cy + 5, textPaint); 

            
            textPaint.TextSize = 12; 
            for (int alt = 30; alt <= 90; alt += 30)
            {
                radius = (alt / 90f) * scale;
                canvas.DrawText($"{90 - alt}°", cx - 15, cy - radius, textPaint);
            }
        }

        private void DrawDsoTrajectory(SKCanvas canvas, float cx, float cy, float scale)
        {
            var trajectoryPaint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            DateTime now = DateTime.UtcNow;
            var astroTimes = Helpers.WeatherRouter.GetAstroTimes(now, true);
            var lat = Helpers.WeatherRouter.lat;
            var lon = Helpers.WeatherRouter.lon;

            
            List<Tuple<float, float>> trajectory = DsoCalculator.calculateDSOpath(
                _selectedDSO, DateTime.UtcNow, astroTimes, lat, lon
            );

            float angleRad = MathF.PI * (0 - 90) / 180f;
            float radius = (1 - (52 / 90f)) * scale;

            float x = cx + MathF.Cos(angleRad) * radius;
            float y = cy + MathF.Sin(angleRad) * radius;

            canvas.DrawCircle(x, y, 3, trajectoryPaint);

            trajectoryPaint = new SKPaint
            {
                Color = SKColors.Red,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            foreach (var (az, alt) in trajectory)
            {
                 angleRad = MathF.PI * (az-90) / 180f;

                 radius = (1 - (alt / 90f)) * scale;

                 x = cx + MathF.Cos(angleRad) * radius;
                 y = cy + MathF.Sin(angleRad) * radius;

                canvas.DrawCircle(x, y, 2, trajectoryPaint);
            }
        }




    }
}