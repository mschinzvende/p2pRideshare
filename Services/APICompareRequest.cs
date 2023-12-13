namespace p2pRideshare.Services
{
    public class APICompareRequest
    {
        public virtual string encoded_image1 { get; set; }
        public virtual string encoded_image2 { get; set; }
        /// <summary>
        /// Optional integer value between 21 and 100. If this parameter added in request 
        /// then uploaded faces quality will be compared from request qualityThreshold value,
        /// otherwise quality check as per defined MXFace standard.
        /// </summary>
        public int? QualityThreshold { get; set; } = null;
    }
}
