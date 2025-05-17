using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;

namespace Video_Share_Project
{

    internal class Video
    {
        public LibVLC libvlc;
        public MediaPlayer mediaPlayer;
        public bool fullscreen = false;
        public bool playing = false;
        public VideoView videoView;
        private Media currentMedia;

        public Video(VideoView view)
        {
            Core.Initialize(); //initializes libVLC package

            libvlc = new LibVLC();
            mediaPlayer = new MediaPlayer(libvlc);
            videoView = view;
            videoView.MediaPlayer = mediaPlayer;

            mediaPlayer.EndReached += EndReached;

            mediaPlayer.EncounteredError += (s, e) =>
            {
                Console.WriteLine("VLC encountered an error!");
            };




            currentMedia = new Media(libvlc, "C:\\Users\\mdond\\Downloads\\zerotofive.mp4", FromType.FromPath);
            mediaPlayer.Play(currentMedia);
            //mediaPlayer.Play(new Media(libvlc, new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4")));
        }



        private void EndReached(object sender, EventArgs e)
        {

            Console.WriteLine("END REACHED");
            Console.WriteLine($"InvokeRequired in EndReached: {videoView.InvokeRequired}");
            if (videoView.InvokeRequired)
            {
                videoView.Invoke(new Action(async () => await PlayNextSegment()));
            }

        }

        private async Task PlayNextSegment()
        {
            await Task.Run(() =>
            {
                Console.WriteLine("skib");
                Console.WriteLine($"InvokeRequired in PlayNextSegment: {videoView.InvokeRequired}");
                currentMedia = new Media(libvlc, "C:\\Users\\mdond\\Downloads\\fivetoten.mp4", FromType.FromPath);
                try
                {
                    Console.WriteLine("dsa");
                    mediaPlayer.Media = currentMedia;
                    Console.WriteLine("sdf");
                    mediaPlayer.Play(currentMedia);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Console.WriteLine("idi");

            });
        }
    }
}
