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
        private async void OnComputeClicked(object sender, EventArgs e)
        {
            
            await AstroWeather.Helpers.DsoCalculator.UpdateYamlFileAsync("GaryImm.yaml", dsoList =>
            {
                var dsoToUpdate = dsoList.FirstOrDefault(d => d.Name == _selectedDSO.Name);
                if (dsoToUpdate != null)
                {
                    dsoToUpdate.photo = true;
                }
            });
        }
        private void OnCanvasPaint(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;
            canvas.Clear(SKColors.Black);


            float centerX = Convert.ToSingle( info.Width / 2);
            float centerY = Convert.ToSingle(info.Height / 2);
            float scale = Math.Min(info.Width, info.Height) / 2.2f;

            DrawAzimuthalGrid(canvas, centerX, centerY, scale);
            DrawPolaris(canvas, centerX, centerY, scale);
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
        private void DrawPolaris(SKCanvas canvas, float cx, float cy, float scale)
        {
            var polarisPaint = new SKPaint
            {
                Color = SKColors.Yellow,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            double lat = Helpers.WeatherRouter.Lat;
            float azPolaris = 0f;
            float altPolaris = Convert.ToSingle(lat);

            float angleRad = MathF.PI * (azPolaris - 90) / 180f;
            float radius = (1 - (altPolaris / 90f)) * scale;

            float x = cx + MathF.Cos(angleRad) * radius;
            var y = cy + MathF.Sin(angleRad) * radius;
            canvas.DrawCircle(x, y, 5, polarisPaint);

            var textPaint = new SKPaint
            {
                Color = SKColors.Yellow,
                TextSize = 20,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };
            canvas.DrawText("Polaris", x, y - 10, textPaint);
        }

        private void DrawDsoTrajectory(SKCanvas canvas, float cx, float cy, float scale)
        {
            var trajectoryPaint = new SKPaint
            {
                Color = SKColors.Red,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            DateTime now = DateTime.UtcNow;
            var astroTimes = Helpers.WeatherRouter.GetAstroTimes(now, false);
            var lat = Helpers.WeatherRouter.Lat;
            var lon = Helpers.WeatherRouter.Lon;

            List<Tuple<float, float>> trajectory = AstroWeather.Helpers.DsoCalculator.calculateDSOpath(
                _selectedDSO, DateTime.UtcNow, astroTimes, lat, lon
            );
            trajectory = trajectory.OrderBy(t => t.Item1).ToList();
            foreach (var (az, alt) in trajectory)
            {
                
                var angleRad = MathF.PI * (az-90) / 180f;
                var alt1 = Math.Clamp(alt, 0, 90);
                var radius = (1 - (alt1 / 90f)) * scale;

                var x = cx + MathF.Cos(angleRad) * radius;
                var y = cy + MathF.Sin(angleRad) * radius;
                canvas.DrawCircle(x, y, 3, trajectoryPaint);
            }
        }
    }
}