using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Tesseract;
using Microsoft.Extensions.Logging;

namespace ORCPO.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OcrController : ControllerBase
    {
        //private readonly ILogger<OcrController> _logger;
        private readonly string _tessdataPath;

        public OcrController()
        {
            //_logger = logger;
            _tessdataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
        }

        /// <summary>
        /// Extract text from an uploaded image using Tesseract OCR.
        /// </summary>
        /// <param name="file">The image file to extract text from.</param>
        /// <returns>Extracted text from the image.</returns>
        [HttpPost("extract-text")]
        public async Task<IActionResult> ExtractTextFromImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            try
            {
                using var engine = new TesseractEngine(_tessdataPath, "eng+hin", EngineMode.Default);
                using var img = Pix.LoadFromMemory(memoryStream.ToArray());
                using var page = engine.Process(img);
                var extractedText = page.GetText();

                //_logger.LogInformation("OCR extraction successful.");
                return Ok(new { Text = extractedText });
            }
            catch (TesseractException ex)
            {
               // _logger.LogError($"Tesseract initialization failed: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, "Failed to process the image. Check tessdata path and language files.");
            }
        }
    }
}
