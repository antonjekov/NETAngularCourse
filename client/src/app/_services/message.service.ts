import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Message } from '../_models/message';
import { getPaginatedResult, GetPaginationHeaders } from './paginationHelper';
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { User } from '../_models/user';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, switchMap, take } from 'rxjs/operators';
import { Group } from '../_models/group';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;
  hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  constructor(private http: HttpClient) { }

  createHubConnection(user: User, otherUsername: string) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${this.hubUrl}message?user=${otherUsername}`, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .catch(error => console.log(error));

    this.hubConnection.on("ReceiveMessageThread", (messages) => {
      this.messageThreadSource.next(messages);
    })

    this.hubConnection.on("NewMessage", (message) => {
      this.messageThread$.pipe(take(1)).subscribe(messages => {
        this.messageThreadSource.next([...messages, message]);
      })
    })

    this.hubConnection.on("UpdatedGroup", (group: Group) => {
      if (group.connections.some(c => c.username === otherUsername)) {
        this.messageThread$
          .pipe(take(1))
          .subscribe(messages => {
            messages.forEach(m => {
              if (!m.dateRead) {
                m.dateRead = new Date(Date.now());
              }
            })
            this.messageThreadSource.next([...messages])
          })
      }
    })
  }

  stopHubConnection() {
    if (this.hubConnection) {
      this.hubConnection
        .stop()
        .catch(error => console.log(error));
    }
  }

  async sendMessage(recipientUsername: string, content: string) {
    // return this.http.post<Message>(`${this.baseUrl}messages`, {recipientUsername, content})
    try {
      return await this.hubConnection
        .invoke("SendMessage", { recipientUsername, content });
    } catch (error) {
      return console.log(error);
    }
  }

  getMessages(pageNumber: number, pageSize: number, container) {
    let params = GetPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);
    return getPaginatedResult<Message[]>(`${this.baseUrl}messages/`, params, this.http);
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(`${this.baseUrl}messages/thread/${username}`);
  }

  deleteMessage(id: number) {
    return this.http.delete(`${this.baseUrl}messages/${id}`)
  }

}
