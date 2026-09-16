import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { useAuth } from '../../hooks/useAuth';

export default function MainIndex() {
    const { user, logout } = useAuth();

    return (
        <View style={styles.container}>
            <Text style={styles.title}>MyAudioVault</Text>
            <Text style={styles.subtitle}>Welcome, {user?.name ?? user?.email}</Text>
            <Text style={styles.role}>Role: {user?.role}</Text>

            <TouchableOpacity style={styles.logoutButton} onPress={logout}>
                <Text style={styles.logoutText}>Log Out</Text>
            </TouchableOpacity>
        </View>
    );
}

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: '#0f0f0f',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 24,
    },
    title: {
        fontSize: 28,
        fontWeight: '700',
        color: '#ffffff',
        marginBottom: 8,
    },
    subtitle: {
        fontSize: 16,
        color: '#d1d5db',
        marginBottom: 4,
    },
    role: {
        fontSize: 14,
        color: '#6b7280',
        marginBottom: 32,
    },
    logoutButton: {
        backgroundColor: '#1f1f1f',
        borderWidth: 1,
        borderColor: '#374151',
        borderRadius: 12,
        paddingVertical: 12,
        paddingHorizontal: 32,
    },
    logoutText: {
        color: '#f87171',
        fontSize: 15,
        fontWeight: '600',
    },
});
