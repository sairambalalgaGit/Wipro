import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MessageUserComponent } from '../message-user/message-user.component';
import { NotificationService } from '../../services/notification.service';
import { NotificationComponent } from '../../shared/notification/notification.component';

@Component({
  selector: 'app-property-details',
  standalone: true,
  imports: [CommonModule, HttpClientModule, FormsModule,MessageUserComponent],
  templateUrl: './property-details.component.html',
  styleUrls: ['./property-details.component.css']
})
export class PropertyDetailsComponent implements OnInit {
  property: any;
  images: string[] = [];
  checkIn: string = '';
  checkOut: string = '';
  apiUrl = 'http://localhost:5101/api/properties';
    @ViewChild('notifier') notifier!: NotificationComponent;
  constructor(private route: ActivatedRoute, private http: HttpClient,private notify: NotificationService) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.http.get(`${this.apiUrl}/${id}`).subscribe({
        next: (res: any) => {
          this.property = res;
          if (this.property.images) {
            this.images = this.property.images.split(',').map((i: string) => i.trim());
            console.log(this.images);
          }
        },
        error: (err) => console.error('Failed to load property', err)
      });
    }
  }

reserveProperty() {
  if (!this.checkIn || !this.checkOut) {
    this.notify.show('❌ Please select check-in and check-out dates', 'error', '');
    return;
  }

  const reservation = {
  propertyId: this.property.propertyId,
  checkIn: this.checkIn,
  checkOut: this.checkOut
};


  console.log("📤 Sending reservation:", reservation);

this.http.post('http://localhost:5101/api/reservations', reservation, {
  headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
}).subscribe({
  next: () => {
    this.notify.show('✅ Reservation created successfully!', 'success', '');
    // alert(res.message || '✅ Reservation created successfully!');
  },
  error: (err) => {
    console.error("❌ Reservation error:", err);

    // ✅ Show backend error
    if (err.error?.message) {
      alert("❌ Failed: " + err.error.message);
    } else if (typeof err.error === 'string') {
      alert("❌ Failed: " + err.error);
    } else {
      alert("❌ Failed: " + err.message);
    }
  }
});

}





}
