   M S B A M L   `   `   `  ����  SkeletonBasics-WPF8    2Microsoft.Samples.Kinect.SkeletonBasics.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   6k(http://schemas.microsoft.com/kinect/2013     5   	   -   5          � Title$
  YOGA��5   	   $	��646��6   $
��1000��6#    � 	Resources 5         MediumGreyBrush �� Color  KinectPurpleBrush  KinectBlueBrush �� 
TargetType �� Property �� Value  SquareCheckBox �� 
TargetType �� Orientation �Name	 ��Name
 ��Name �� TextAlignment �� Triggers P� Property P� Value �� 
TargetName��%1  &	        &	 #     &	 F     '�� i     &	 �     7  7  �� 5	   
   $ 	#ff6e6e6e��62   �� 5
   
   $ 	#ff52318f��64   �� 5   
   $ 	#ff00BCF2��62   �� 5   
   " ��6   .�5      ��  �@J�SnapsToDevicePixels . 64   5      �� 5   
   " ��6   .�5      ��  � �� 5      �� 5      " ��6&   .R�5      � .+�5      �� $ 
Horizontal=�6)   
������ .+�5   "   � $ SquareCheckBoxChecked��6'   .+�5   &   �� $	 CheckedNormal��6,   $��Images\CheckedNormal.png��6C   $
��None=�6e   $��Center=�6t   �� 5   &   $	 CheckedHover��6,   $��Images\CheckedHover.png��6B   $
��None=�6c   $��Center=�6r   $y�	Collapsed=�6�   5   #   � 5   "   $ SquareCheckBoxUnchecked��6'   $y�	Collapsed=�6H   .+�5   &   �� $	 UncheckedNormal��6,   $ ��Images\UncheckedNormal.png��6E   $
��None=�6i   $��Center=�6x   �� 5   &   $	 UncheckedHover��6,   $��Images\UncheckedHover.png��6D   $
��None=�6g   $��Center=�6v   $y�	Collapsed=�6�   5   #   �� 5   "   $
 SquareCheckBoxText��6,   #��z��6H   $
 Left=�6i   $��Center=�6   8��  $��15(�6�   $��9,0,0,0q�6�   5      5       5       P� 5!       �@d�	IsChecked$ falseY�6;   . �5"   "   �� $ SquareCheckBoxChecked��6Q    � y�$ 	Collapsed=�6?   6Q   �� 5#   "   $ SquareCheckBoxUnchecked��6O    � y�$ Visible=�6?   6O   5$      P� 5%       �@J�IsMouseOver . . �5&   "   �� $ CheckedNormal��6Q    � y�$ 	Collapsed=�6?   6Q   �� 5'   "   $ UncheckedNormal��6Q    � y�$ 	Collapsed=�6?   6Q   �� 5(   "   $ CheckedHover��6O    � y�$ Visible=�6?   6O   �� 5)   "   $ UncheckedHover��6O    � y�$ Visible=�6?   6O   �� 5*   "   8  $ SquareCheckBoxText��6h    � ��5+      5-      5/      50      51      .��53      � -   5        $��
layoutGrid��53      $��10,0,2,0q�6   $	��510��6/   .+�54   
   �� -   5        $��Image��54   -   $��0��6   $��0��6   $	��640��6:   $	��480��6F   $��11,10,329,20q�6S   �� 55   
   -   5        $��button��55   q   $��Capture Frame��6   $
��Auto��6)   $��Stretch=�67   $��Stretch=�6U   $	��101��6   $��199,515,680,-25q�6�   �� 56   
   -   5        $��
dogebutton��56   o   $��Toggle Doge��6   $
��Auto��6'   $��Stretch=�65   $��Stretch=�6S   $	��101��6�   $��440,515,440,-25q�6�   �� 57   
   -   5        $��centerbutton��57   {   $��Show Stretch of Balance��6   $
��Auto��63   $��Stretch=�6A   $��Stretch=�6_   $	��148��6�   $��631,515,201,-25q�6�   �� 58   
   -   5        $��	statusBar��58   ?   $��3��6   $��Stretch=�6!   $��Bottom=�6P   
�������#��[  6~   $��2��6�   .@�59      �� $��0 0 0 10q�6   5:      �� 5;   
   -   5        $��	infoImage��5;   _   $	��100��6   $	��350��6   $��Stretch=�6)   $	��Top=�6G   $��292,24,338,0q�6p   �� 5<   
   -	   5        $��Mountain��5<      $��Stretch=�6$   $��44��6B   $��
667,10,0,0q�6N   $	��Top=�6b   $	��300��6z   6�   �� 5=   
   -
   5        $
��Tree��5=      $��Stretch=�6    $��44��6>   $��
667,80,0,0q�6J   $	��Top=�6^   $	��300��6v   6�   �� 5>   
   -   5        $��Upwards Salute��5>      $��Stretch=�6*   $��44��6H   $��667,154,0,0q�6T   $	��Top=�6i   $	��300��6�   6�   �� 5?   
   -   5        $��Warrior Two��5?      $��Stretch=�6'   $��44��6E   $��667,229,0,0q�6Q   $	��Top=�6f   $	��300��6~   6�   �� 5@   
   -   5        $��Reverse Warrior��5@      $��Stretch=�6+   $��44��6I   $��667,302,0,0q�6U   $	��Top=�6j   $	��300��6�   6�   �� 5A   
   -   5        $��Moon Flower��5A      $��Stretch=�6'   $��44��6E   $��667,374,0,0q�6Q   $	��Top=�6f   $	��300��6~   6�   �� 5B   
   -   5        $
��None��5B      $��Stretch=�6    $��44��6>   $��667,446,0,0q�6J   $	��Top=�6_   $	��300��6w   6�   5C      5D      