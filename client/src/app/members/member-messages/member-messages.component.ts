import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {

  @Input() messages: Message[];
  @Input() recipientUsername: string;
  @ViewChild('messageForm') messageForm: NgForm;
  content: string;

  constructor(public messageService: MessageService) { }

  ngOnInit(): void {
  }

  sendMessage() {
    this.messageService.sendMessage(this.recipientUsername, this.content).then(() => {
      this.messageForm.reset();
    });
  }
}
