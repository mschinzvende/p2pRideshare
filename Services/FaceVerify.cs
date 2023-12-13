namespace p2pRideshare.Services
{
    public class FaceVerify
    {
        public short matchResult { get; set; }
        public FaceDetect image1_face { get; set; }
        public FaceDetect image2_face { get; set; }
    }
}
